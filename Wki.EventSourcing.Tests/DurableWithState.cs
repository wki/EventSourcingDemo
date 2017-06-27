using System;
using Akka.Actor;
using Akka.TestKit.NUnit;
using Wki.EventSourcing.Actors;

namespace Wki.EventSourcing.Tests
{
    public partial class DurableRetrievalTest : TestKit
    {
        public class DurableWithState : DurableActor<DurableState>
        {
            public DurableWithState(IActorRef eventStore): base(eventStore) {}

            protected override EventFilter BuildEventFilter() =>
                WantEvents.AnyEvent();

            protected override DurableState BuildInitialState() =>
                new DurableState();

            protected override void Handle(object message) =>
                Sender.Tell($"Reply to '{message}'");
        }
    }
}
