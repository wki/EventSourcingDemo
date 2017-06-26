using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Wki.EventSourcing;
using Wki.EventSourcing.Actors;
using System;
using System.Collections.Generic;
using Wki.EventSourcing.Protocol.Retrieval;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class DurableRetrievalTest : TestKit
    {
        #region durable actor classes
        public class DurableWithoutState : DurableActor
        {
            public DurableWithoutState(IActorRef eventStore) : base(eventStore)
            {
            }

            protected override void Apply(IEvent e)
            {
                // TODO
            }

            protected override EventFilter BuildEventFilter() =>
                WantEvents.AnyEvent();

            protected override void OnReceive(object message)
            {
                // TODO: noch machen
            }
        }

        public class DurableState : IState<DurableState>
        {
            public List<string> AppliedEvents { get; set; } = new List<string>();

            public IState<DurableState> Apply(IEvent @event)
            {
                AppliedEvents.Add(@event.GetType().Name);
                return this;
            }
        }

        public class DurableWithState : DurableActor<DurableState>
        {

            public DurableState State { get; set; }

            public DurableWithState(IActorRef eventStore) : base(eventStore)
            {
                HasState = true; // triggers snapshot loading
                State = new DurableState();
            }

            protected override void Apply(IEvent e) =>
                State.Apply(e);

            protected override EventFilter BuildEventFilter() =>
                WantEvents.AnyEvent();

            protected override void OnReceive(object message)
            {
                // TODO: noch machen
            }

            protected override DurableState BuildInitialState() =>
                new DurableState();
        }
        #endregion

        private TestProbe eventStore;

        [SetUp]
        public void TestSetup()
        {
            eventStore = CreateTestProbe("eventStore");
        }

        [Test]
        public void DurableWithState_Initially_RequestsSnapshot()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithState>(eventStore));

            // Assert
            eventStore.ExpectMsg<LoadSnapshot>(s => s.PersistenceId == "DurableWithState");
        }

        [Test]
        public void DurableWithState_NoSnapshot_RequestsEventsFromBeginning()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithState>(eventStore));

            // Act
            eventStore.FishForMessage<LoadSnapshot>(s => true);
            d.Tell(NoSnapshot.Instance, eventStore);

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(l => !l.EventFilter.FiltersEvents && l.EventFilter.StartAfterEventId == -1);
        }

        [Test]
        public void DurableWithState_WithSnapshot_RequestsEventsAfterSnapshot()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithState>(eventStore));

            // Act
            eventStore.FishForMessage<LoadSnapshot>(s => true);
            d.Tell(new Snapshot(new DurableState(), 41), eventStore);

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(l => !l.EventFilter.FiltersEvents && l.EventFilter.StartAfterEventId == 41);
        }

        [Test]
        public void DurableWithState_SnapshotTimeout_RequestsEventsFromBeginning()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithState>(eventStore));

            // Act
            eventStore.FishForMessage<LoadSnapshot>(s => true);
            d.Tell(ReceiveTimeout.Instance, d);

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(l => !l.EventFilter.FiltersEvents && l.EventFilter.StartAfterEventId == -1);
        }

        [Test]
        public void DurableWithoutState_Initially_RequestEventsFromBeginning()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithoutState>(eventStore));

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(l => !l.EventFilter.FiltersEvents && l.EventFilter.StartAfterEventId == -1);
        }
    }
}
