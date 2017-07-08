using System;
using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using FakeItEasy;
using Wki.EventSourcing.Persistence;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Tests.Actors;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class JournalPersistenceTest: TestKit
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
        public void Journal_PersistEvent_AppendsEventToJournalStore()
        {
            // Arrange
            var @event = new DurableState.SomethingHappened();

            // Act
            journal.Tell(new PersistEvent("xxx", @event));

            // Assert
            A.CallTo(() => journalStore.AppendEvent("xxx", @event))
                .MustHaveHappened();
        }

        [Test]
        public void Journal_PersistEvent_RepliesEventPersisted()
        {
            // Arrange
            journalStore.LastEventId = 42;
            var @event = new DurableState.SomethingHappened();

            // Act
            journal.Tell(new PersistEvent("foo", @event));

            // Assert
            ExpectMsg<EventPersisted>(eventPersisted =>
            {
                Console.WriteLine("PersistenceId={0}", eventPersisted.EventRecord.PersistenceId);
                Console.WriteLine("Event Type={0}", eventPersisted.EventRecord.Event.GetType().FullName);
                Console.WriteLine("Id={0}", eventPersisted.EventRecord.Id);

                return eventPersisted.EventRecord.PersistenceId == "foo"
                    && eventPersisted.EventRecord.Event.GetType() == typeof(DurableState.SomethingHappened)
                    && eventPersisted.EventRecord.Id == 42;
            });
        }

        [Test]
        public void Journal_PersistEventFail_RepliesPersistEventFailed()
        {
            // Arrange
            A.CallTo(() => journalStore.AppendEvent(A<string>._, A<IEvent>._))
                .Throws(new ArgumentException("foo failed"));
            var @event = new DurableState.SomethingHappened();

            // Act
            journal.Tell(new PersistEvent("xxx", @event));

            // Assert
            ExpectMsg<PersistEventFailed>(failed =>
            {
                Console.WriteLine("Message={0}", failed.Message);
                Console.WriteLine("PersistenceId={0}", failed.PersistenceId);
                Console.WriteLine("Type={0}", failed.Event.GetType());

                return failed.Message == "foo failed"
                    && failed.PersistenceId == "xxx"
                    && failed.Event.GetType() == typeof(DurableState.SomethingHappened);
            });
        }

        [Test]
        public void Journal_PersistSnapshot_CallsSaveSnapshot()
        {
            // Act
            journal.Tell(new PersistSnapshot("xxx", new DurableState(), 77));

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
            // hint: call happens asynchronous -- so we must wait...
            A.CallTo(() => journalStore.SaveSnapshot("xxx", A<DurableState>._, 77))
                .MustHaveHappened();
        }

        [Test]
        public void Journal_PersistSnapshotFail_NothingHappens()
        {
            // Arrange
            A.CallTo(() => journalStore.SaveSnapshot(A<string>._, A<DurableState>._, A<int>._))
                .Throws(new ArgumentException());

            // Act
            journal.Tell(new PersistSnapshot("xxx", new DurableState(), 88));

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }
    }
}
