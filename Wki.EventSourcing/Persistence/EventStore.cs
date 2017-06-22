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
                    // simply forward -- we are not interested.
                    Journal.Forward(loadSnapshot);
                    break;

                case LoadNextEvents loadNextEvents:
                    var wantEvents = Subscriptions
                        .EventsWantedFor(Sender)
                        .StartingAfterEventId(loadNextEvents.EventFilter.StartAfterEventId);
                    foreach (var eventRecord in EventCache.NextEventsMatching(wantEvents, loadNextEvents.NrEvents))
                        Sender.Tell(eventRecord);
                    break;
            }
        }

        // Loading cache behavior
        private void Loading(object message)
        {
            switch(message)
            {
                case EventRecord eventRecord:
                    EventCache.Append(eventRecord);
                    if (--nrEventsExpected == 0)
                    {
                        var l = LoadNextEvents.After(EventCache.LastId);
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

        // Persisting behavior
        private void Persisting(object message)
        {
            switch(message)
            {
                case EventPersisted eventPersisted:
                    DispatchToSubscribers(eventPersisted.EventRecord);
                    EventCache.Append(eventPersisted.EventRecord);
                    Stash.UnstashAll();
                    Become(OnReceive);
                    break;

                case PersistEventFailed persistEventFailed:
                    LastPersistingActor.Tell(persistEventFailed);
                    Stash.UnstashAll();
                    Become(OnReceive);
                    break;

                // TODO: Timeout ?

                default:
                    Stash.Stash();
                    break;
            }
        }

        private void DispatchToSubscribers(EventRecord eventRecord)
        {
            foreach (var subscriber in Subscriptions.ActorsSubscribedFor(eventRecord))
                subscriber.Tell(eventRecord);
        }
    }
}
