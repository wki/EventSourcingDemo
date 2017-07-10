using System;
using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using FakeItEasy;
using Wki.EventSourcing.Persistence;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Tests.Actors;
using Wki.EventSourcing.Protocol.Retrieval;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class JournalRehydrateTest: TestKit
    {
        private IJournalStore journalStore;
        private IActorRef journal;

        [SetUp]
        public void SetUp()
        {
            journalStore = A.Fake<IJournalStore>();
            journal = Sys.ActorOf(Props.Create<Journal>(journalStore), "journal");
        }

        [Test]
        public void Journal_LoadSnapshot_CallsHasSnapshot()
        {
            // Act
            journal.Tell(new LoadSnapshot("xxx", typeof(DurableState)));

            // Assert
            A.CallTo(() => journalStore.HasSnapshot("xxx"))
                .MustHaveHappened();
        }

        [Test]
        public void Journal_LoadSnapshotNotExisting_RepliesNoShapshot()
        {
            // Arrange
            A.CallTo(() => journalStore.HasSnapshot("xxx"))
                .Returns(false);

            // Act
            journal.Tell(new LoadSnapshot("xxx", typeof(DurableState)));

            // Assert
            ExpectMsg<NoSnapshot>(_ => true);
        }

        [Test]
        public void Journal_LoadSnapshotExisting_RepliesSnapshot()
        {
            // Arrange
            A.CallTo(() => journalStore.HasSnapshot("xxx"))
                .Returns(true);
            A.CallTo(() => journalStore.LoadSnapshot("xxx", typeof(DurableState)))
                .Returns(new Snapshot(new DurableState(), 11));

            // Act
            journal.Tell(new LoadSnapshot("xxx", typeof(DurableState)));

            // Assert
            ExpectMsg<Snapshot>(s => s.LastEventId == 11 && s.State.GetType() == typeof(DurableState));
        }

        [Test]
        public void Journal_LoadSnapshotDies_RepliesNoShapshot()
        {
            // Arrange
            A.CallTo(() => journalStore.HasSnapshot("xxx"))
                .Returns(true);
            A.CallTo(() => journalStore.LoadSnapshot("xxx", typeof(DurableState)))
                .Throws<ArgumentNullException>();

            // Act
            journal.Tell(new LoadSnapshot("xxx", typeof(DurableState)));

            // Assert
            ExpectMsg<NoSnapshot>();
        }
    }
}
