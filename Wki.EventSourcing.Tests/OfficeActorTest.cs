using System;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Tests.Messages;
using Wki.EventSourcing.Protocol.Statistics;

namespace Wki.EventSourcing.Tests
{
    public class OfficeActorTest : TestKit
    {
        private IActorRef officeActor;
        private TestProbe eventStore;

        public class Foo : ReceiveActor
        {
            public IActorRef EventStore { get; set; }
            public int Number { get; set; }

            public Foo(IActorRef eventStore, int number)
            {
                EventStore = eventStore;
                Number = number;
            
                Receive<SomeCommand>(s => Sender.Tell($"Hello from {s.Id}"));
            }
        }

        public class FooOffice : OfficeActor<Foo, int>
        {
            
            public FooOffice(IActorRef eventStore) : base(eventStore)
            {
                // do not handly any command. 
                // Base class or Foo does the trick.

                // diagnostic reply to a non-command message
                Receive<string>(s => Sender.Tell($"Reply to {s}"));
            }
        }

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("event-store");
            officeActor = Sys.ActorOf(Props.Create<FooOffice>(eventStore), "foo-office");
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void OfficeActor_Initially_HasNoChildren()
        {
            // Act
            officeActor.Tell(new GetStatistics());

            // Assert
            ExpectMsg<OfficeActorStatistics>(s => s.NrActorsLoaded == 0);
        }

        [Test]
        public void OfficeActor_NonCommandMessage_IsHandledByOffice()
        {
            // Act
            officeActor.Tell("huhu");

            // Assert
            ExpectMsg<string>("Reply to huhu");
        
            // Act #2 -- expect no children
            officeActor.Tell(new GetStatistics());

            // Assert #2
            ExpectMsg<OfficeActorStatistics>(s => s.NrActorsLoaded == 0);
        }

        // DispatchableCommand --> neuen Actor anlegen, weiterleiten
        [Test]
        public void OfficeActor_CommandMessage_IsHandledByChild()
        {
            // Act
            officeActor.Tell(new SomeCommand(42));

            // Assert
            ExpectMsg<string>("Hello from 42");

            // Act #2 -- expect one child
            officeActor.Tell(new GetStatistics());

            // Assert #2
            ExpectMsg<OfficeActorStatistics>(s => s.NrActorsLoaded == 1);
        }
    }
}
