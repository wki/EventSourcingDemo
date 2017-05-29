using Akka.Actor;
using Wki.EventSourcing.Protocol;

namespace Wki.EventSourcing.Actors
{
    public abstract class SubscribingActor: UntypedActor
    {
        public IActorRef EventStore;

        public SubscribingActor(IActorRef eventStore)
        {
            EventStore = eventStore;
        }

        protected void Subscribe() =>
            Subscribe(BuildEventFilter());

        protected void Subscribe(EventFilter eventFilter) =>
            EventStore.Tell(new Subscribe(eventFilter));

        protected void UnSubscribe() =>
            EventStore.Tell(new Unsubscribe());

        // must be overloaded in order to return events we are interested in
        protected abstract EventFilter BuildEventFilter();
    }
}
