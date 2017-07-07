using System;
using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Wki.EventSourcing.Tests.Actors;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Protocol.Persistence;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class DurablePersistTest: TestKit
    {
        private TestProbe eventStore;
        private IActorRef durable;

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("eventStore");
            durable = Sys.ActorOf(Props.Create<DurableWithState>(eventStore), "durable");   
        }

        private void CompleteRestorePhase()
        {
            eventStore.FishForMessage<LoadSnapshot>(_ => true);
            durable.Tell(NoSnapshot.Instance, eventStore);
            eventStore.FishForMessage<LoadNextEvents>(_ => true);
            durable.Tell(End.Instance, eventStore);
        }

        [Test]
        public void Durable_CommandPersistsEvent_CallsEventStore()
        {
            // Arrange
            CompleteRestorePhase();

            // Act: command in actor calls persist
            durable.Tell(new DurableState.LetSomethingHappen());

            // Assert
            eventStore.ExpectMsg<PersistEvent>(_ => true);
        }

        [Test]
        public void Durable_CommandPersistEvent_RepliesSuccess()
        {
            // Arrange
            CompleteRestorePhase();

            // Act
            durable.Tell(new DurableState.LetSomethingHappen());
            eventStore.ExpectMsg<PersistEvent>(_ => true);
            durable.Tell(
                new EventRecord(42, DateTime.Now, "Xxx", new DurableState.SomethingHappened())
            );

            // Assert
            ExpectMsg<Reply>(reply => reply.IsOk);
        }

        [Test]
        public void Durable_CommandPersistEventTimeout_RepliesError()
        {
            // Arrange
            CompleteRestorePhase();

            // Act
            durable.Tell(new DurableState.LetSomethingHappen());
            eventStore.ExpectMsg<PersistEvent>(_ => true);
            // fake a timeout without waiting for 2 seconds
            durable.Tell(ReceiveTimeout.Instance);

            // Assert
            ExpectMsg<Reply>(reply => !reply.IsOk);
        }

        // Exception -> actor --> Reply:Error
        [Test]
        public void Durable_CommandPersistEventError_RepliesError()
        {
            // Arrange
            CompleteRestorePhase();

            // Act
            durable.Tell(new DurableState.LetSomethingHappen());
            eventStore.ExpectMsg<PersistEvent>(_ => true);
            durable.Tell(new PersistEventFailed("foo", new DurableState.SomethingHappened(), "really bad"), eventStore);

            // Assert
            ExpectMsg<Reply>(reply => 
                {
                    Console.WriteLine("received message '{0}'", reply.Message);
                    return !reply.IsOk && reply.Message == "really bad";
                }
            );
        }

        // actor -> PersistSnapshot --> (nothing)
    }
}
