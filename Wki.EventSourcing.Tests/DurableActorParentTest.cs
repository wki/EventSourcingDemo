using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using System;
using System.Threading;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.LiveCycle;
using Wki.EventSourcing.Protocol.Misc;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Tests
{
    public class CrashingDurableActor : DurableActor
    {
        public class SomethingBadHappened : Event { }
        public class SomethingGoodHappened : Event { }

        public CrashingDurableActor(IActorRef eventStore) : base(eventStore)
        {
            passivationTimeSpan = TimeSpan.FromMinutes(1);

            Recover<SomethingGoodHappened>(_ => { });
            Recover<SomethingBadHappened>(_ => { throw new ArgumentException("huhu"); });
        }
    }

    public class SimpleParentActor : ReceiveActor
    {
        public class IsAlive { }
        public class DontCrash { }
        public class LetCrash { }

        private IActorRef durableActor;
        private bool durableIsAlive;

        private IActorRef rememberedSender;

        public SimpleParentActor(IActorRef eventStore)
        {
            durableActor = Context.ActorOf(Props.Create<CrashingDurableActor>(eventStore));
            durableIsAlive = true;

            // requests sent to us from test cases below
            Receive<IsAlive>(_ => Sender.Tell(durableIsAlive));
            Receive<DontCrash>(_ => DontDurableCrash());
            Receive<LetCrash>(_ => LetDurableCrash());
            Receive<EndOfTransmission>(e => durableActor.Tell(e));
            Receive<Tick>(t => ForwardTick(t));

            // answers from durable forwarded to test case
            Receive<DiedDuringRestore>(d => rememberedSender.Tell(d));
            Receive<StillAlive>(s => rememberedSender.Tell(s));
            Receive<Passivate>(p => rememberedSender.Tell(p));
        }

        private void DontDurableCrash()
        {
            rememberedSender = Sender;
            durableActor.Tell(new CrashingDurableActor.SomethingGoodHappened());
        }

        private void LetDurableCrash()
        {
            rememberedSender = Sender;
            durableActor.Tell(new CrashingDurableActor.SomethingBadHappened());
        }

        private void ForwardTick(Tick t)
        {
            rememberedSender = Sender;
            durableActor.Tell(new EndOfTransmission());
            durableActor.Tell(t);
        }
    }

    [TestFixture]
    public class DurableActorParentTest : TestKit
    {
        private TestProbe eventStore;
        private IActorRef parent;

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("eventStore");
            parent = Sys.ActorOf(Props.Create<SimpleParentActor>(eventStore));
        }

        [Test]
        public void DurableActorParent_Initially_DurableIsAlive()
        {
            // Act
            parent.Tell(new SimpleParentActor.IsAlive());

            // Assert
            ExpectMsg<bool>(isAlive => isAlive == true);
        }

        [Test]
        public void DurableActorParent_LivesDuringRestore_DoesNotReceiveException()
        {
            // Act
            parent.Tell(new SimpleParentActor.DontCrash());

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void DurableActorParent_DiesDuringRestore_ReceivesException()
        {
            // Act
            parent.Tell(new SimpleParentActor.LetCrash());

            // Assert
            ExpectMsg<DiedDuringRestore>(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void DurableActorParent_TimerFiredBeforePassivation_ReceivesStillAlive()
        {
            // Act
            parent.Tell(new Tick());

            // Assert
            ExpectMsg<StillAlive>(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void DurableActorParent_TimerFiredAfterPassivation_ReceivesPassivate()
        {
            // Arrange
            parent.Tell(new EndOfTransmission());
            Thread.Sleep(TimeSpan.FromSeconds(0.5)); // not fine but ensure actor is started.
            SystemTime.Fake(() => DateTime.Now + TimeSpan.FromMinutes(1.5));

            // Act
            parent.Tell(new Tick());

            // Assert
            ExpectMsg<Passivate>(TimeSpan.FromSeconds(0.5));
        }
    }
}
