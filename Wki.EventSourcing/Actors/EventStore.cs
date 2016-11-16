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

        // maintain a complete state (for knowledge and statistics)
        private EventStoreState eventStoreState;

        public EventStore(string dir, IActorRef reader = null, IActorRef writer = null)
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

            eventStoreState = new EventStoreState();
            eventsToReceive = 0;

            actors = new Dictionary<string,ActorState>();
            events = new List<Event>();

            Become(Loading);
        }

        #region Loading State
        // junk-wise request events from journal reader
        private void Loading()
        {
            Context.System.Log.Info("Loading all events from event store");

            eventStoreState.ChangeStatus(EventStoreStatus.Loading);

            Receive<EventLoaded>(e => EventLoaded(e));
            Receive<End>(_ => Become(Operating));

            // diagnostic messages for monitoring
            Receive<GetStatusReport>(_ => ReplyStatusReport());
            Receive<GetState>(_ => Sender.Tell(eventStoreState));

            // could be a command - keep for later
            Receive<object>(_ =>
            {
                eventStoreState.NrStashedCommands++;
                Stash.Stash();
            });

            RequestEventsToLoad();
        }

        // readJournal loaded an event -- save it in our list
        private void EventLoaded(EventLoaded eventLoaded)
        {
            eventStoreState.NrEventsLoaded++;
            eventStoreState.NrEventsTotal++;
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
            eventStoreState.ChangeStatus(EventStoreStatus.Operating);
            eventStoreState.LoadDuration = SystemTime.Now - eventStoreState.StartedAt;

            Context.System.Log.Info(
                "{0} Events loaded in {1:N3}s. Starting regular operation", 
                eventStoreState.NrEventsLoaded,
                eventStoreState.LoadDuration.TotalSeconds
            );

            // periodically check for inactive children
            Context.System.Scheduler
                   .ScheduleTellRepeatedly(
                       initialDelay: IdleActorPollTimeSpan,
                       interval:     IdleActorPollTimeSpan,
                       receiver:     Self,
                       message:      new RemoveInactiveActors(),
                       sender:       Self
                   );

            // messages from actors
            Receive<StartRestore>(s => StartRestore(s));
            Receive<RestoreEvents>(r => RestoreEvents(r));
            Receive<StillAlive>(_ => StillAlive());
            Receive<NotAlive>(_ => NotAlive());
            Receive<PersistEvent>(e => PersistEvent(e));

            // message from journal writer
            Receive<EventPersisted>(p => EventPersisted(p));

            // diagnostic messages for testing
            Receive<GetSize>(_ => Sender.Tell(events.Count));
            Receive<GetActors>(_ => Sender.Tell(String.Join("|", actors.Values.Select(a => a.ToString()))));
            Receive<RemoveInactiveActors>(_ => RemoveInactiveActors());

            // diagnostic messages for monitoring
            Receive<GetStatusReport>(_ => ReplyStatusReport());
            Receive<GetState>(_ => Sender.Tell(eventStoreState));

            // process everything lost so far
            Stash.UnstashAll();
        }

        // an actor wants to get restored
        private void StartRestore(StartRestore startRestore)
        {
            eventStoreState.NrActorsRestored++;

            var path = Sender.Path.ToString();
            actors[path] = new ActorState(path, Sender, startRestore.InterestingEvents);

            eventStoreState.NrSubscribers = actors.Count;
        }

        // an actor wants n more Events
        private void RestoreEvents(RestoreEvents restoreEvents)
        {
            var path = Sender.Path.ToString();

            if (!actors.ContainsKey(path))
            {
                // should not happen but silently ignore this unlikely happening request
                return;
            }

            var actorState = actors[path];
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
            var path = Sender.Path.ToString();
            eventStoreState.NrStillAliveReceived++;

            if (actors.ContainsKey(path))
            {
                var actorState = actors[path];
                actorState.LastSeen = SystemTime.Now;
            }
        }

        // an actor tells us that he just died
        private void NotAlive()
        {
            var path = Sender.Path.ToString();
            actors.Remove(path);

            Context.System.Log.Info("Actor {0} just died, removed", path);
        }

        private void PersistEvent(object @event)
        {
            journalWriter.Tell(@event);
        }

        private void RemoveInactiveActors()
        {
            var oldestAllowedTime = SystemTime.Now - MaxActorIdleTimeSpan;

            Context.System.Log.Info("Event Store - Checking actors. Oldest time: {0:HH:mm}", oldestAllowedTime);
            // actors.Values.ToList().ForEach(a => Console.WriteLine($"{a.Path} - {a.LastSeen}"));

            actors
                .Values
                .Where(actor => actor.LastSeen < oldestAllowedTime)
                .ToList()
                .ForEach(actor =>
                {
                    Context.System.Log.Info(
                        "Removing actor {0}, last Seen {1} seconds ago",
                        actor.Path, (SystemTime.Now - actor.LastSeen).TotalSeconds
                    );
                    actors.Remove(actor.Path);
                });

            eventStoreState.NrSubscribers = actors.Count;
        }

        // writeHournal persisted an event -- forward it to all actors interested in it
        private void EventPersisted(EventPersisted eventPersistet)
        {
            eventStoreState.NrEventsPersisted++;
            eventStoreState.NrEventsTotal++;
            eventStoreState.LastEventPersistedAt = SystemTime.Now;

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

        #region status report (common to both states)
        // generate ans reply status report
        private void ReplyStatusReport()
        {
            var actorStates =
                actors.Values
                      .Select(a => new StatusReport.ActorStatus 
                            {
                                Path = a.Path,
                                Status = a.Restoring ? "Restoring" : "Operating",
                                LastSeen = a.LastSeen,
                                Events = a.InterestingEvents.Events.Select(e => e.Name).ToList(),
                            })
                      .ToList();

            var statusReport = new StatusReport
            {
                EventStoreState = eventStoreState,
                Actors = actorStates,
            };

            Sender.Tell(statusReport);
        }
        #endregion
    }
}
