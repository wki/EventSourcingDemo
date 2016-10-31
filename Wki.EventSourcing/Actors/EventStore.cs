using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;

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
            public string Path { get; set; }
            public IActorRef Actor { get; set; }
            public bool Restoring { get; set; }
            public DateTime LastSeen { get; set; }
            public int NextMessageIndex { get; set; }
            public InterestingEvents InterestingEvents { get; set; }

            public ActorState(string path, IActorRef actor, InterestingEvents interestingEvents)
            {
                Path = path;
                Actor = actor;
                Restoring = true;
                LastSeen = SystemTime.Now;
                NextMessageIndex = 0;
                InterestingEvents = interestingEvents;
            }

            public bool InterestedIn(Event @event) => InterestingEvents.Matches(@event);

            public override string ToString()
            {
                var restoreFlag = Restoring ? ">" : "=";
                var age = (SystemTime.Now - LastSeen).TotalSeconds;

                return string.Format($"{restoreFlag}{Path}-{age:N0}");
            }
        }

        public IStash Stash { get; set; }

        // during restore count events for aquiring next junk before completion
        private int eventsToReceive;

        // responsible for writing persisted events to persistent storage
        private IActorRef journalWriter;

        // responsible for reading all persisted events from storage
        private IActorRef journalReader;

        // list of all actors (Path => ActorState) we are in contact with
        private Dictionary<string, ActorState> actors;

        // complete event list in memory for fast access
        private List<Event> events;

        // actor start time to measure durations
        private DateTime startedAt;

        public EventStore(string dir, IActorRef reader, IActorRef writer)
        {
            var config = Context.System.Settings.Config.GetConfig("eventstore");

            journalReader = reader;
            journalWriter = writer;

            if (config != null)
            {
                var storageDir = dir ?? config.GetString("dir");
                Context.System.Log.Info("Storage Dir: {0}", storageDir);

                var readerTypeName = config.GetString("reader");
                var readerType = Type.GetType(readerTypeName ?? "") ?? typeof(FileJournalReader);
                journalReader = reader ?? Context.ActorOf(Props.Create(readerType, storageDir), "reader");

                var writerTypeName = config.GetString("writer");
                var writerType = Type.GetType(writerTypeName ?? "") ?? typeof(FileJournalWriter);
                journalWriter = writer ?? Context.ActorOf(Props.Create(writerType, storageDir), "writer");
            }

            startedAt = SystemTime.Now;

            actors = new Dictionary<string,ActorState>();
            events = new List<Event>();

            Become(Loading);
        }

        #region Loading State
        private void Loading()
        {
            Context.System.Log.Info("Loading all events from event store");

            Receive<EventLoaded>(e => EventLoaded(e));
            Receive<End>(_ => Become(Operating));
            Receive<object>(_ => Stash.Stash());

            eventsToReceive = 0;
            RequestEventsToLoad();
        }

        // readJournal loaded an event -- save it in our list
        private void EventLoaded(EventLoaded eventLoaded)
        {
            eventsToReceive--;

            events.Add(eventLoaded.Event);
            RequestEventsToLoad();
        }

        private void RequestEventsToLoad()
        {
            if (eventsToReceive <= BufferLowLimit)
            {
                eventsToReceive += NrRestoreEvents;
                journalReader.Tell(new LoadJournal(NrRestoreEvents));
            }
        }
        #endregion

        #region Operating state
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

            // diagnostic messages for testing
            Receive<GetSize>(_ => Sender.Tell(events.Count));
            Receive<GetActors>(_ => Sender.Tell(String.Join("|", actors.Values.Select(a => a.ToString()))));
            Receive<CheckInactiveActors>(_ => CheckInactiveActors());

            // process everything lost so far
            Stash.UnstashAll();
        
            // TODO: start timer to monitor lost actors
        }
        // an actor wants to get restored
        private void StartRestore(StartRestore startRestore)
        {
            // hint: might override existing entry.
            actors[Sender.Path.ToString()] = new ActorState(Sender.Path.ToString(), Sender, startRestore.InterestingEvents);
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
                var @event = events[index];
                if (actorState.InterestingEvents.Matches(@event))
                {
                    Sender.Tell(@event);
                    nrEvents--;
                }
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

        // remove all actors not alive for a long time
        private void CheckInactiveActors()
        {
            // TODO: maybe we should send a "ping" to an actor not responding
            //       for some time. If nothing happens then, we remove it.
            var oldestAllowedTime = SystemTime.Now - TimeSpan.FromSeconds(MaxActorIdleSeconds);

            // Console.WriteLine($"Oldest time: {oldestAllowedTime}");
            // actors.Values.ToList().ForEach(a => Console.WriteLine($"{a.Path} - {a.LastSeen}"));

            actors
                .Values
                .Where(actor => actor.LastSeen < oldestAllowedTime)
                .ToList()
                .ForEach(actor =>
                {
                    Context.System.Log.Debug(
                        "Removing actor {0}, last Seen {1} seconds ago",
                        actor.Path, (SystemTime.Now - actor.LastSeen).TotalSeconds
                    );
                    actors.Remove(actor.Path);
                });
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
        #endregion
    }
}
