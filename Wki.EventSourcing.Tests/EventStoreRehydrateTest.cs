using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Wki.EventSourcing.Persistence;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Tests.Actors;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class EventStoreRehydrateTest: TestKit
    {
        private TestProbe journal;
        private IActorRef eventStore;

        [SetUp]
        public void SetUp()
        {
            journal = CreateTestProbe("journal");
            eventStore = Sys.ActorOf(Props.Create<EventStore>(journal), "eventStore");
        }

        private void CompleteLoadPhase()
        {
            journal.FishForMessage<LoadNextEvents>(_ => true);
            eventStore.Tell(End.Instance);
        }

        [Test]
        public void EventStore_LoadSnapshot_ForwardsToJournal()
        {
            // Arrange
            CompleteLoadPhase();

            // Act
            eventStore.Tell(new LoadSnapshot("xxx", typeof(DurableState)));

            // Assert
            journal.ExpectMsg<LoadSnapshot>(l => l.PersistenceId == "xxx" && l.StateType == typeof(DurableState));
        }


    }
}
