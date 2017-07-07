using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using System;
using Wki.EventSourcing.Protocol.Retrieval;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Tests.Actors;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public partial class DurableRetrievalTest : TestKit
    {
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
            eventStore.ExpectMsg<LoadNextEvents>(LoadEventsFromBeginning);
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
            eventStore.ExpectMsg<LoadNextEvents>(LoadEventsFromBeginning);
        }

        private bool LoadEventsFromBeginning(LoadNextEvents loadNextEvents)
        {
            if (loadNextEvents.EventFilter.FiltersEvents)
            {
                Console.WriteLine("Filtering events -- expected: no");
                return false;
            }

            if (loadNextEvents.EventFilter.StartAfterEventId != -1)
            {
                Console.WriteLine("Starting after {0}, expected -1", loadNextEvents.EventFilter.StartAfterEventId);
                return false;
            }

            if (loadNextEvents.NrEvents != DefaultNrEvents)
            {
                Console.WriteLine("Requested {0} events, expected {1}", loadNextEvents.NrEvents, DefaultNrEvents);
                return false;
            }

            return true;
        }

        [Test]
        public void DurableWithoutState_Initially_RequestEventsFromBeginning()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithoutState>(eventStore));

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(LoadEventsFromBeginning);
        }

        [Test]
        public void DurableWithoutState_LoadEvents_RegularOperationAfterEnd()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithoutState>(eventStore));
            eventStore.FishForMessage<LoadNextEvents>(_ => true);

            // Act
            d.Tell("huhu"); // must be deferred
            d.Tell(new EventRecord(1, DateTime.Now, "", new DurableState.SomethingHappened()));
            d.Tell(new EventRecord(2, DateTime.Now, "", new DurableState.FooHandled()));
            d.Tell(new EventRecord(7, DateTime.Now, "", new DurableState.FooHandled()));
            d.Tell(End.Instance);

            // Assert
            ExpectMsg<string>("Reply to Event 'SomethingHappened' 1");
            ExpectMsg<string>("Reply to Event 'FooHandled' 2");
            ExpectMsg<string>("Reply to Event 'FooHandled' 7");

        }

        [Test]
        public void DurableWithoutState_LoadEvents_RequestMoreWhenProcessed()
        {
            // Arrange
            var d = Sys.ActorOf(Props.Create<DurableWithoutState>(eventStore));
            eventStore.FishForMessage<LoadNextEvents>(_ => true);

            // Act
            d.Tell("huhu"); // must be deferred
            for (var i = 1; i <= DefaultNrEvents; i++)
                d.Tell(new EventRecord(i, DateTime.Now, "", new DurableState.FooHandled()));

            // Assert
            eventStore.ExpectMsg<LoadNextEvents>(l => l.EventFilter.StartAfterEventId == DefaultNrEvents);
        }
    }
}
