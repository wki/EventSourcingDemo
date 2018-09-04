using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Protocol.Subscription;

namespace Wki.EventSourcing.Persistence
{
    public class EventStore : UntypedActor, IWithUnboundedStash
    {
        public IStash Stash { get; set; }
        public IActorRef Journal;
        public Subscriptions Subscriptions = new Subscriptions();
        public EventCache EventCache = new EventCache();

        // set before switching to Persisting for telling "PersistEventFailed"
        public IActorRef LastPersistingActor;

        public int nrEventsExpected;

        public EventStore(IActorRef journal)
        {
            Journal = journal;

            var loadNextEvents = LoadNextEvents.FromBeginning();
            nrEventsExpected = loadNextEvents.NrEvents;
            Journal.Tell(loadNextEvents);

            Become(Loading);
        }

        // regular behavior
        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case PersistEvent persistEvent:
                    // we do not forward because we want the reply.
                    Journal.Tell(persistEvent);
                    LastPersistingActor = Sender;
                    Become(Persisting);
                    break;

                case PersistSnapshot persistSnapshot:
                    Journal.Forward(persistSnapshot);
                    break;

                case Subscribe subscribe:
                    Subscriptions.Subscribe(Sender, subscribe.EventFilter);

                    // catch up persists that happened since this actor's restore
                    // usually we expect just a few but we request _all_
                    foreach (var eventRecord in EventCache.NextEventsMatching(subscribe.EventFilter, Int32.MaxValue))
                        Sender.Tell(eventRecord);
                    break;

                case Unsubscribe _:
                    Subscriptions.Unsubscribe(Sender);
                    break;

                case LoadSnapshot loadSnapshot:
                    HandleLoadSnapshot(loadSnapshot);
                    break;

                case LoadNextEvents loadNextEvents:
                    HandleLoadNextEvents(loadNextEvents);
                    break;
            }
        }

        // Loading cache behavior happening initially
        private void Loading(object message)
        {
            switch(message)
            {
                case EventRecord eventRecord:
                    EventCache.Add(eventRecord);
                    if (--nrEventsExpected == 0)
                    {
                        var l = Protocol.Retrieval.LoadNextEvents.After(EventCache.LastId);
                        nrEventsExpected = l.NrEvents;
                        Journal.Tell(l);
                    }
                    break;

                case End _:
                    Stash.UnstashAll();
                    Become(OnReceive);
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }

        // Persisting behavior -- do not persist anything else while waiting for an actor to be persisted
        // also do not forward any messages to subscribers while waiting for persist to be done
        // but: load events and snapshots
        private void Persisting(object message)
        {
            switch(message)
            {
                case EventPersisted eventPersisted:
                    DispatchToSubscribers(eventPersisted.EventRecord);
                    EventCache.Add(eventPersisted.EventRecord);
                    Stash.UnstashAll();
                    Become(OnReceive);
                    break;

                case PersistEventFailed persistEventFailed:
                    LastPersistingActor.Tell(persistEventFailed);
                    Stash.UnstashAll();
                    Become(OnReceive);
                    break;
                
                    // TODO: Timeout ?

                case LoadSnapshot loadSnapshot:
                    HandleLoadSnapshot(loadSnapshot);
                    break;

                case LoadNextEvents loadNextEvents:
                    HandleLoadNextEvents(loadNextEvents);
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }

        private void HandleLoadNextEvents(LoadNextEvents loadNextEvents)
        {
            int nrEventsSent = 0;
            foreach (var eventRecord in EventCache.NextEventsMatching(loadNextEvents.EventFilter, loadNextEvents.NrEvents))
            {
                Sender.Tell(eventRecord);
                nrEventsSent++;
            }

            if (nrEventsSent < loadNextEvents.NrEvents)
                Sender.Tell(End.Instance);
        }

        private void HandleLoadSnapshot(LoadSnapshot loadSnapshot)
        {
            // simply forward -- we are not interested.
            Journal.Forward(loadSnapshot);
        }

        private void DispatchToSubscribers(EventRecord eventRecord)
        {
            foreach (var subscriber in Subscriptions.ActorsSubscribedFor(eventRecord))
                subscriber.Tell(eventRecord);
        }
    }
}
