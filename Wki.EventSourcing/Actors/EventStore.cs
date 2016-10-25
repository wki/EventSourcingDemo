using System;
using System.Collections.Generic;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Global Event Store keeping all events in Memory
    /// </summary>
    public class EventStore : ReceiveActor, IWithUnboundedStash
    {
        // hold information about an actor we are in contact with
        private class ActorState
        {
            public IActorRef Actor { get; set; }
            public bool Restoring { get; set; }
            public DateTime LastSeen { get; set; }
            public int NextMessageIndex { get; set; }
            public InterestingEvents InterestingEvents { get; set; }

            public ActorState(IActorRef actor, InterestingEvents interestingEvents)
            {
                Actor = actor;
                Restoring = true;
                LastSeen = SystemTime.Now;
                NextMessageIndex = 0;
                InterestingEvents = interestingEvents;
            }

            public bool InterestedIn(Event @event) => InterestingEvents.Matches(@event);
        }

        public IStash Stash { get; set; }

        // responsible for writing persisted events to persistent storage
        private IActorRef journalWriter;

        // responsible for reading all persisted events from storage
        private IActorRef journalReader;

        // list of all actors (Path => ActorState) we are in contact with
        private Dictionary<string,ActorState> actors;

        // complete event list in memory for fast access
        private List<Event> events;

        // actor start time to measure durations
        private DateTime startedAt;

        public EventStore()
        {
            var config = Context.System.Settings.Config.GetConfig("eventstore");
            var storageDir = config.GetString("dir");
            Context.System.Log.Info("Storage Dir:D {0}", storageDir);

            journalReader = Context.ActorOf(Props.Create<FileJournalReader>(storageDir), "reader");
            journalWriter = Context.ActorOf(Props.Create<FileJournalWriter>(storageDir), "writer");
            journalReader.Tell(new LoadJournal());

            startedAt = SystemTime.Now;

            actors = new Dictionary<string,ActorState>();
            events = new List<Event>();

            Become(Loading);
        }

        #region states
        private void Loading()
        {
            Context.System.Log.Info("Loading all events from event store");

            Receive<EventLoaded>(e => EventLoaded(e));
            Receive<End>(_ => Become(Operating));
            Receive<object>(_ => Stash.Stash());
        }

        private void Operating()
        {
            var loadDuration = SystemTime.Now - startedAt;
            Context.System.Log.Info("Events loaded in {0:F1}s . Starting regular operation", loadDuration.TotalMilliseconds / 1000);

            // messages from actors
            Receive<StartRestore>(s => StartRestore(s));
            Receive<RestoreEvents>(r => RestoreEvents(r));
            Receive<StillAlive>(_ => StillAlive());
            Receive<PersistEvent>(p => journalWriter.Tell(p));

            // message from journal writer
            Receive<EventPersisted>(p => EventPersisted(p));

            // process everything lost so far
            Stash.UnstashAll();
        
            // TODO: start timer to monitor lost actors
        }
        #endregion

        // an actor wants to get restored
        private void StartRestore(StartRestore startRestore)
        {
            // hint: might override existing entry.
            actors[Sender.Path.ToString()] = new ActorState(Sender, startRestore.InterestingEvents);
        }

        // an actor wants n more Events
        private void RestoreEvents(RestoreEvents restoreEvents)
        {
            var actorState = actors[Sender.Path.ToString()];
            actorState.LastSeen = SystemTime.Now;

            var nrEvents = restoreEvents.NrEvents;
            var index = actorState.NextMessageIndex; // next index to test and send

            while (nrEvents > 0 && index < events.Count)
            {
                Sender.Tell(events[index]);
                nrEvents--;
                index++;
                actorState.NextMessageIndex++;
            }

            if (actorState.NextMessageIndex >= events.Count)
            {
                // we reached the end
                Sender.Tell(new End());
                actorState.Restoring = false;
            }
        }

        // an actor tells it is still alive
        private void StillAlive()
        {
            var actorState = actors[Sender.Path.ToString()];
            actorState.LastSeen = SystemTime.Now;
        }

        // readJournal loaded an event -- save it in our list
        private void EventLoaded(EventLoaded eventLoaded)
        {
            events.Add(eventLoaded.Event);
        }

        // writeHournal persisted an event -- forward it to all actors interested in it
        private void EventPersisted(EventPersisted eventPersistet)
        {
            var @event = eventPersistet.Event;

            // add for all running and subsequential restore operations
            events.Add(@event);

            foreach (var actor in actors.Values)
            {
                // restoring actors are omitted because they will receive the event added above
                if (!actor.Restoring && actor.InterestedIn(@event))
                    actor.Actor.Tell(@event);
            }
        }
    }
}
