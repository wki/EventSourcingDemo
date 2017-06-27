using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Wki.EventSourcing.Tests
{
    public class DurableWithoutState : DurableActor
    {
        public DurableWithoutState(IActorRef eventStore) : base(eventStore) { }

        protected override void Apply(IEvent e) { }

        protected override EventFilter BuildEventFilter() =>
            WantEvents.AnyEvent();

        protected override void Handle(object message) =>
            Sender.Tell($"Reply to '{message}' {LastEventId}");
    }
}
