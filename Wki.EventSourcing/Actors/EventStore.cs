using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.Subscription;
using Wki.EventSourcing.Protocol.LiveCycle;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Global Event Store keeping all events in Memory
    /// </summary>
    public class EventStore : ReceiveActor, IWithUnboundedStash
    {
        // hold information about an actor we are in contact with
        private class SubscriberState
        {
            public string Path { get; set; }
            public IActorRef Actor { get; set; }
            public bool Restoring { get; set; }
            public DateTime LastSeen { get; set; }
            public int NextMessageIndex { get; set; }
            public InterestingEvents InterestingEvents { get; set; }

            public SubscriberState(string path, IActorRef actor, InterestingEvents interestingEvents)
            {
                Path = path;
                Actor = actor;
                Restoring = true;
                LastSeen = SystemTime.Now;
                NextMessageIndex = 0;
                InterestingEvents = interestingEvents;
            }

            public bool IsInterestedIn(Event @event) => InterestingEvents.Matches(@event);

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
        private Dictionary<string, SubscriberState> subscribers;

        // complete event list in memory for fast access
        private List<Event> events;

        // maintain a complete state (for knowledge and statistics)
        private EventStoreStatistics eventStoreStatistics;

        public EventStore(string dir, IActorRef reader = null, IActorRef writer = null)
        {
            var config = Context.System.Settings.Config.GetConfig("eventstore");

            journalReader = reader;
            journalWriter = writer;


            eventStoreStatistics = new EventStoreStatistics();
            eventsToReceive = 0;

            subscribers = new Dictionary<string,SubscriberState>();
            events = new List<Event>();

            Become(Loading);
        }

        #region Loading State
        // junk-wise request events from journal reader
        private void Loading()
        {
            Context.System.Log.Info("Loading all events from event store");

            eventStoreStatistics.ChangeStatus(EventStoreStatus.Loading);

            // Protocol: fill event store
            Receive<EventLoaded>(e => EventLoaded(e));
            Receive<End>(_ => Become(Operating));

            // diagnostic messages for monitoring
            Receive<GetStatusReport>(_ => ReplyStatusReport());
            Receive<GetStatistics>(_ => Sender.Tell(eventStoreStatistics));

            // could be a command - keep for later
            Receive<object>(_ =>
            {
                eventStoreStatistics.NrStashedCommands++;
                Stash.Stash();
            });

            RequestEventsToLoad();
        }

        // readJournal loaded an event -- save it in our list
        private void EventLoaded(EventLoaded eventLoaded)
        {
            eventStoreStatistics.NrEventsLoaded++;
            eventStoreStatistics.NrEventsTotal++;
            eventsToReceive--;

            events.Add(eventLoaded.Event);
            RequestEventsToLoad();
        }

        private void RequestEventsToLoad()
        {
            if (eventsToReceive <= BufferLowLimit)
            {
                eventsToReceive += NrRestoreEvents;
                journalReader.Tell(new LoadNextEvents(NrRestoreEvents));
            }
        }
        #endregion

        #region Operating state
        private void Operating()
        {
            eventStoreStatistics.ChangeStatus(EventStoreStatus.Operating);
            eventStoreStatistics.LoadDuration = SystemTime.Now - eventStoreStatistics.StartedAt;

            Context.System.Log.Info(
                "{0} Events loaded in {1:N3}s. Starting regular operation", 
                eventStoreStatistics.NrEventsLoaded,
                eventStoreStatistics.LoadDuration.TotalSeconds
            );

            //// periodically check for inactive children
            //Context.System.Scheduler
            //       .ScheduleTellRepeatedly(
            //           initialDelay: IdleActorPollTimeSpan,
            //           interval:     IdleActorPollTimeSpan,
            //           receiver:     Self,
            //           message:      new RemoveInactiveActors(),
            //           sender:       Self
            //       );

            // Protocol: Load Durable Actor
            Receive<StartRestore>(s => StartRestore(s));
            Receive<RestoreNextEvents>(r => RestoreEvents(r));

            // Procol: Subscribe/Unsubscribe
            Receive<Subscribe>(s => Subscribe(s));
            Receive<Unsubscribe>(_ => Unsubscribe());

            // Protocol: Persist Event
            Receive<PersistEvent>(e => PersistEvent(e));
            Receive<EventPersisted>(p => EventPersisted(p));

            // diagnostic messages for testing
            Receive<GetActors>(_ => Sender.Tell(String.Join("|", subscribers.Values.Select(a => a.ToString()))));

            // diagnostic messages for monitoring
            Receive<GetStatusReport>(_ => ReplyStatusReport());
            Receive<GetStatistics>(_ => Sender.Tell(eventStoreStatistics));

            // process everything lost so far
            Stash.UnstashAll();
        }

        // an actor wants to get restored
        // TODO: macht eigentlich nichts. Notwendig?
        private void StartRestore(StartRestore startRestore)
        {
            eventStoreStatistics.NrActorsRestored++;
        }

        // an actor wants n more Events
        private void RestoreEvents(RestoreNextEvents restoreEvents)
        {
            var path = Sender.Path.ToString();

            if (!subscribers.ContainsKey(path))
            {
                // should not happen but silently ignore this unlikely happening request
                return;
            }

            var actorState = subscribers[path];
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

        // subscribe an actor to its requested events
        private void Subscribe(Subscribe subscribe)
        {
            var path = Sender.Path.ToString();
            if (!subscribers.ContainsKey(path))
            {
                Context.System.Log.Debug("Subscribe {0}: {1}", path, subscribe.InterestingEvents.ToString());
                var subscriberState = new SubscriberState(path, Sender, subscribe.InterestingEvents);
                subscribers.Add(path, subscriberState);

                eventStoreStatistics.NrSubscribers = subscribers.Count;
            }
        }

        // unsubscribe an actor
        private void Unsubscribe()
        {
            var path = Sender.Path.Address.ToString();
            if (subscribers.ContainsKey(path))
            {
                Context.System.Log.Debug("Unsubscribe {0}", path);
                subscribers.Remove(path);

                eventStoreStatistics.NrSubscribers = subscribers.Count;
            }
        }

        private void PersistEvent(object @event)
        {
            // TODO: hier evtl. weitere Angaben weiter reichen
            // z.B. Sender
            // TODO: oder überlegen, Envelopes weiter zu geben
            journalWriter.Tell(@event);
        }

        // writeHournal persisted an event -- forward it to all actors interested in it
        private void EventPersisted(EventPersisted eventPersistet)
        {
            eventStoreStatistics.NrEventsPersisted++;
            eventStoreStatistics.NrEventsTotal++;
            eventStoreStatistics.LastEventPersistedAt = SystemTime.Now;

            var @event = eventPersistet.Event;

            // add for all running and subsequential restore operations
            events.Add(@event);

            foreach (var actor in subscribers.Values)
            {
                // restoring actors are omitted because they will receive the event added above
                if (!actor.Restoring && actor.IsInterestedIn(@event))
                    actor.Actor.Tell(@event);
            }
        }
        #endregion

        #region status report (common to both states)
        // generate ans reply status report
        private void ReplyStatusReport()
        {
            var actorStates =
                subscribers.Values
                      .Select(a => new StatusReport.ActorStatus 
                            {
                                Path = a.Path,
                                Status = a.Restoring ? "Restoring" : "Operating",
                                LastSeen = a.LastSeen,
                                Events = a.InterestingEvents.Events.Select(e => e.Name).OrderBy(e => e).ToList(),
                            })
                      .ToList();

            var statusReport = new StatusReport
            {
                EventStoreStatistics = eventStoreStatistics,
                Actors = actorStates,
            };

            Sender.Tell(statusReport);
        }
        #endregion
    }
}
