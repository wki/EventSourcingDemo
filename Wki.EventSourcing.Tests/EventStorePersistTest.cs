using System;
using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Wki.EventSourcing.Tests.Actors;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Persistence;
using Wki.EventSourcing.Protocol.Subscription;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class EventStorePersistTest: TestKit
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
        public void EventStore_PersistEventSuccess_RepliesEventRecordToSubscribingSender()
        {
            // Arrange
            CompleteLoadPhase();
            var other = CreateTestProbe("other");
            eventStore.Tell(new Subscribe(WantEvents.All()));
            eventStore.Tell(new Subscribe(WantEvents.NoEvent()), other);

            // Act
            eventStore.Tell(new PersistEvent("xxx", new DurableState.SomethingHappened()));

            // Assert
            journal.ExpectMsg<PersistEvent>();
            eventStore.Tell(
                new EventPersisted(
                    new EventRecord(1, DateTime.Now, "xxx", new DurableState.SomethingHappened())
                ),
                journal
            );
            ExpectMsg<EventRecord>(eventRecord =>
            {
                Console.WriteLine("Event Type={0}", eventRecord.Event.GetType().FullName);
                return eventRecord.Event.GetType() == typeof(DurableState.SomethingHappened);
            });
            other.ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void EventStore_PersistEventSuccess_RepliesEventRecordToSubscribers()
        {
            // Arrange
            CompleteLoadPhase();
            var other = CreateTestProbe("other");
            eventStore.Tell(new Subscribe(WantEvents.All()));
            eventStore.Tell(new Subscribe(WantEvents.All()), other);

            // Act
            eventStore.Tell(new PersistEvent("xxx", new DurableState.SomethingHappened()));

            // Assert
            journal.ExpectMsg<PersistEvent>();
            eventStore.Tell(
                new EventPersisted(
                    new EventRecord(1, DateTime.Now, "xxx", new DurableState.SomethingHappened())
                ),
                journal
            );
            ExpectMsg<EventRecord>(eventRecord =>
            {
                Console.WriteLine("Event Type={0}", eventRecord.Event.GetType().FullName);
                return eventRecord.Event.GetType() == typeof(DurableState.SomethingHappened);
            });
            other.ExpectMsg<EventRecord>(eventRecord => eventRecord.Event.GetType() == typeof(DurableState.SomethingHappened));
        }

        [Test]
        public void EventStore_PersistEventError_RepliesFaildToSender()
        {
            // Arrange
            CompleteLoadPhase();
            var other = CreateTestProbe("other");
            eventStore.Tell(new Subscribe(WantEvents.All()));
            eventStore.Tell(new Subscribe(WantEvents.All()), other);

            // Act
            eventStore.Tell(new PersistEvent("xxx", new DurableState.SomethingHappened()));

            // Assert
            journal.ExpectMsg<PersistEvent>();
            eventStore.Tell(
                new PersistEventFailed("xxx", new DurableState.SomethingHappened(), "it did not work")
            );
            ExpectMsg<PersistEventFailed>(failed =>
            {
                Console.WriteLine("Event Type={0}", failed.Event.GetType().FullName);
                Console.WriteLine("Message={0}", failed.Message);
                Console.WriteLine("PersistenceId={0}", failed.PersistenceId);

                return failed.Event.GetType() == typeof(DurableState.SomethingHappened)
                    && failed.Message == "it did not work"
                    && failed.PersistenceId == "xxx";
            });
            other.ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void EventStore_PersistSnapshot_ForwardedToJournal()
        {
            // Arrange
            CompleteLoadPhase();

            // Act
            eventStore.Tell(new PersistSnapshot("xxx", new DurableState(), 22));

            // Assert
            journal.ExpectMsg<PersistSnapshot>(persistSnapshot =>
            {
                Console.WriteLine("State={0}", persistSnapshot.State.GetType().FullName);
                Console.WriteLine("PersistenceId={0}", persistSnapshot.PersistenceId);
                Console.WriteLine("LastEventId={0}", persistSnapshot.LastEventId);
                return persistSnapshot.State.GetType() == typeof(DurableState)
                    && persistSnapshot.LastEventId == 22
                    && persistSnapshot.PersistenceId == "xxx";
            });
        }
    }
}
