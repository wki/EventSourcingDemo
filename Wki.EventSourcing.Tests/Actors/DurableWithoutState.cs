using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Wki.EventSourcing.Tests.Actors
{
    public class DurableWithoutState : DurableActor
    {
        public DurableWithoutState(IActorRef eventStore) : base(eventStore) { }

        protected override EventFilter BuildEventFilter() =>
            WantEvents.AnyEvent();

        protected override void Handle(object message) =>
            Sender.Tell($"Reply to Message '{message.GetType().Name}' {LastEventId}");

        protected override void HandleCommand(ICommand command) =>
            Persist(new DurableState.SomethingHappened());

        protected override void ApplyEvent(IEvent @event) =>
            Sender.Tell($"Reply to Event '{@event.GetType().Name}' {LastEventId}");
    }
}
