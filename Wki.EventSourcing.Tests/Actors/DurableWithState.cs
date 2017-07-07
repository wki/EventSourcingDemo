using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Wki.EventSourcing.Tests.Actors
{
    public class DurableWithState : DurableActor<DurableState>
    {
        public DurableWithState(IActorRef eventStore) : base(eventStore) {}

        protected override EventFilter BuildEventFilter() =>
            WantEvents.AnyEvent();

        protected override DurableState BuildInitialState() =>
            new DurableState();

        protected override void Handle(object message)
        {
            // Sender.Tell($"Reply to '{message}' {LastEventId}");

            switch(message)
            {
                case DurableState.LetSomethingHappen l:
                    Persist(new DurableState.SomethingHappened());
                    break;

                case DurableState.HandleFoo h:
                    Persist(new DurableState.FooHandled());
                    break;

                case DurableState.GetState _:
                    Sender.Tell(State);
                    break;
            }
        }
    }
}
