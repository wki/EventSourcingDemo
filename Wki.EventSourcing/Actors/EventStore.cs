using System.Collections.Generic;
using Akka.Actor;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.Subscription;
using System;
using System.Linq;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Global Event Store keeping all events in Memory
    /// </summary>
    public class EventStore : ReceiveActor, IWithUnboundedStash
    {
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
        private EventStoreStatistics statistics;

        public EventStore(string dir, IActorRef reader, IActorRef writer)
        {
            // maybe some day we read from config
            // var config = Context.System.Settings.Config.GetConfig("eventstore");

            journalReader = reader ?? throw new ArgumentNullException(nameof(reader));
            journalWriter = writer ?? throw new ArgumentNullException(nameof(writer));

            statistics = new EventStoreStatistics();
            eventsToReceive = 0;

            subscribers = new Dictionary<string, SubscriberState>();
            events = new List<Event>();

            Become(Loading);
        }

        #region Loading State
        // junk-wise request events from journal reader
        private void Loading()
        {
            Context.System.Log.Debug("Loading all events from event store");

            statistics.ChangeStatus(EventStoreStatus.Loading);

            // Protocol: fill event store
            Receive<EventLoaded>(e => EventLoaded(e));
            Receive<EndOfTransmission>(_ => Become(Operating));

            // diagnostic messages for monitoring
            Receive<GetStatistics>(_ => Sender.Tell(statistics));

            // could be a command - keep for later
            Receive<object>(_ =>
            {
                statistics.StashedCommand();
                Stash.Stash();
            });

            RequestEventsToLoad();
        }

        // readJournal loaded an event -- save it in our list
        private void EventLoaded(EventLoaded eventLoaded)
        {
            statistics.LoadedEvent();
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
            statistics.ChangeStatus(EventStoreStatus.Operating);

            Context.System.Log.Info(
                "{0} Events loaded in {1:N3}s. Starting regular operation",
                statistics.NrEventsLoaded,
                statistics.LoadDuration.TotalSeconds
            );

            // Protocol: Load Durable Actor
            Receive<StartRestore>(s => StartRestore(s));
            Receive<RestoreNextEvents>(r => RestoreEvents(r));

            // Procol: Subscribe/Unsubscribe
            Receive<Subscribe>(s => Subscribe(s));
            Receive<Unsubscribe>(_ => Unsubscribe());

            // Protocol: Persist Event
            Receive<PersistEvent>(e => PersistEvent(e));
            Receive<EventPersisted>(p => EventPersisted(p));

            // diagnostic messages for monitoring
            Receive<GetStatistics>(_ => Sender.Tell(statistics));

            // process everything lost so far
            Stash.UnstashAll();
        }

        // an actor wants to get restored
        // TODO: macht eigentlich nichts. Notwendig?
        private void StartRestore(StartRestore startRestore)
        {
            statistics.StartRestore();
        }

        // an actor wants n more Events
        private void RestoreEvents(RestoreNextEvents restoreEvents)
        {
            var path = Sender.Path.ToString();

            if (!subscribers.ContainsKey(path))
            {
                Context.System.Log.Warning(
                    "trying to restore not subscribing actor {0} -- ignored",
                    path
                );
                return;
            }

            var subscriber = subscribers[path];
            subscriber.LastSent = SystemTime.Now;

            var nrEvents = restoreEvents.NrEvents;
            var index = subscriber.NextMessageIndex;

            while (nrEvents > 0 && index < events.Count)
            {
                var @event = events[index];
                if (subscriber.IsInterestedIn(@event))
                {
                    Sender.Tell(@event);
                    nrEvents--;
                }
                index++;
            }
            subscriber.NextMessageIndex = index;

            if (subscriber.NextMessageIndex >= events.Count)
            {
                // we reached the end
                Sender.Tell(new EndOfTransmission());
                subscriber.Restoring = false;
            }
        }

        // subscribe an actor to its requested events
        private void Subscribe(Subscribe subscribe)
        {
            var path = Sender.Path.ToString();
            if (subscribers.ContainsKey(path))
                Context.System.Log.Warning(
                    "trying to subscribe already subscribing actor {0} -- ignored",
                    path
                );
            else
            {
                Context.System.Log.Debug(
                    "Subscribe {0}: {1}",
                    path, subscribe.InterestingEvents
                );
                var subscriberState = new SubscriberState(Sender, subscribe.InterestingEvents);
                subscribers.Add(path, subscriberState);

                statistics.NrSubscribers = subscribers.Count;
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

                statistics.NrSubscribers = subscribers.Count;
            }
            else
                Context.System.Log.Warning(
                    "trying to unsubscribe not subscribing actor {0} -- ignored",
                    path
                );
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
            statistics.PersistedEvent();

            var @event = eventPersistet.Event;

            // add for all running and subsequential restore operations
            events.Add(@event);

            foreach (var subscriber in subscribers.Values.Where(s => s.IsInterestedIn(@event)))
            {
                if (subscriber.Restoring)
                    // restoring actors will receive the event during their restore cycle
                    Context.System.Log.Debug(
                        "trying to send {0} to restoring actor {1} -- stashed",
                        @event.GetType().Name, subscriber.Actor.Path
                    );
                else
                {
                    subscriber.Actor.Tell(@event);
                    subscriber.ReceivedEvent();
                }
            }
        }
        #endregion
    }
}
