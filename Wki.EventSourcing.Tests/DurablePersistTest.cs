using System;
using NUnit.Framework;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;

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

        // command -> actor --> event -> eventStore

        // eventRecord -> actor --> actor -> Reply:Success

        // Timeout -> actor --> Reply:Error

        // Exception -> actor --> Reply:Error

        // actor -> PersistSnapshot --> (nothing)
    }
}
