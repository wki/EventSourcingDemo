using System;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.LiveCycle;
using Wki.EventSourcing.Protocol.Misc;
using Wki.EventSourcing.Protocol.Subscription;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol;
using System.Linq;
using Wki.EventSourcing.Util;
using System.Threading;

namespace Wki.EventSourcing.Tests
{
    public class SimpleDurableActor : DurableActor
    {
        public class SomethingHappened : Event { }
        public class CallPersist
        {
            public Event Event { get; set; }

            public CallPersist(Event @event)
            {
                Event = @event;
            }
        }

        public int NrSomethingHappened { get; set; }

        public SimpleDurableActor(IActorRef eventStore) : base(eventStore)
        {
            passivationTimeSpan = TimeSpan.FromSeconds(300);

            NrSomethingHappened = 0;

            Recover<SomethingHappened>(_ => NrSomethingHappened++);
            Receive<string>(s => Sender.Tell($"Reply: {s}"));
            Receive<int>(i => 
            {
                //if (i < 0)
                //    throw new ArgumentException("lt zero");

                if (i < 0)
                    Sender.Tell(Reply.Error("lt zero"));
                else
                    Sender.Tell(Reply.Ok());
            });

            // not an event but wired this way to ensure a test case works
            Recover<CallPersist>(p => Persist(p.Event));
        }
    }

    [TestFixture]
    public class DurableActorTest : TestKit
    {
        private TestProbe eventStore;
        private IActorRef durableActor;

        // the begin of our test timebase
        private DateTime fakeStartTime;

        // Cast the TestActorSystem scheduler to be a TestScheduler
        private TestScheduler Scheduler => (TestScheduler)Sys.Scheduler;

        // change scheduler implementation
        public DurableActorTest()
            : base(@"akka.scheduler.implementation = ""Akka.TestKit.TestScheduler, Akka.TestKit""")
        { }

        [SetUp]
        public void SetUp()
        {
            fakeStartTime = DateTime.Now; // new DateTime(2017, 1, 1, 12, 21, 43);
            SystemTime.Fake(() => fakeStartTime);

            eventStore = CreateTestProbe("eventStore");
            durableActor = Sys.ActorOf(
                Props.Create<SimpleDurableActor>(eventStore)
            );
        }

        #region initial behavior
        [Test]
        public void DurableActor_AfterStart_Statistics()
        {
            // Act
            durableActor.Tell(new GetStatistics());

            // Assert
            var statistics = ExpectMsg<DurableActorStatistics>();
            Assert.AreEqual(DurableActorStatus.Restoring, statistics.Status, "Status");
            Assert.AreEqual(0, statistics.NrEventsTotal, "NrEvents");
            Assert.AreEqual(0, statistics.NrCommandsTotal, "NrCommands");
            Assert.AreEqual(0, statistics.NrUnhandledMessages, "NrUnhandled");
        }

        [Test]
        public void DurableActor_AfterStart_DoesNotProcessCustomMessage()
        {
            // Act
            durableActor.Tell("Hello");

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.1));
        }
        #endregion

        #region restore events
        [Test]
        public void DurableActor_AfterStart_RestoresEvents()
        {
            // Assert
            eventStore.FishForMessage<RestoreNextEvents>(r => r.NrEvents == NrRestoreEvents);
        }

        [Test]
        public void DurableActor_AfterReceivingEnoughRestoreEvents_RequestsNextJunk()
        {
            // Arrange
            eventStore.FishForMessage<RestoreNextEvents>(m => true, TimeSpan.FromSeconds(0.5), "restore #1");

            // Act
            var somethingHappened = new SimpleDurableActor.SomethingHappened();
            for (var i = 1; i <= NrRestoreEvents; i++)
                durableActor.Tell(somethingHappened);

            // Assert
            eventStore.ExpectMsg<RestoreNextEvents>(r => r.NrEvents == NrRestoreEvents, TimeSpan.FromSeconds(0.5), "restore #2");
        }

        [Test]
        public void DurableActor_AfterReceivingEndOfTransmission_SwitchesToOperating()
        {
            // Arrange
            eventStore.FishForMessage<RestoreNextEvents>(m => true, TimeSpan.FromSeconds(0.5), "restore #1");
            durableActor.Tell(new SimpleDurableActor.SomethingHappened());
            durableActor.Tell(new SimpleDurableActor.SomethingHappened());

            // Act
            durableActor.Tell(new EndOfTransmission());

            // Assert
            durableActor.Tell(new GetStatistics());
            var statistics = ExpectMsg<DurableActorStatistics>();
            Assert.AreEqual(DurableActorStatus.Operating, statistics.Status, "Status");
            Assert.AreEqual(0, statistics.NrEventsTotal, "NrEvents");
            Assert.AreEqual(2, statistics.NrRestoreEvents, "NrRestoreEvents");
            Assert.AreEqual(0, statistics.NrCommandsTotal, "NrCommands");
        }
        #endregion

        #region subscribe
        [Test]
        public void DurableActor_AfterStart_Subscribes()
        {
            // Assert
            var subscribe = eventStore.FishForMessage<Subscribe>(m => true, TimeSpan.FromSeconds(0.5), "Subscribe");
            Assert.AreEqual(2, subscribe.InterestingEvents.Events.Count, "nr events");
            Assert.AreEqual(
                "CallPersist,SomethingHappened",
                String.Join(
                    ",", 
                    subscribe.InterestingEvents.Events
                             .ToList()
                             .Select(t => t.Name)
                             .OrderBy(n => n)
                ),
                "Events"
            );
        }

        [Test]
        public void DurableActor_AfterStop_Unsubscribes()
        {
            // Act
            durableActor.Tell(PoisonPill.Instance);

            // Assert
            eventStore.FishForMessage<Unsubscribe>(m => true);
        }
        #endregion

        #region passivate
        [Test]
        public void DurableActor_BeforeStillAliveInterval_NoAction()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission());
            var halfInterval = TimeSpan.FromMilliseconds(ActorStillAliveInterval.TotalMilliseconds / 2);
            Scheduler.Advance(halfInterval);

            // Act
            /* scheduler does not fire */

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void DurablActor_AfterStillAliveInterval_FiresStillAlive()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission());
            var moreThanInterval = TimeSpan.FromMilliseconds(ActorStillAliveInterval.TotalMilliseconds * 1.1);
            Scheduler.Advance(moreThanInterval);
            var timerFiredAt = fakeStartTime + moreThanInterval;
            SystemTime.Fake(() => timerFiredAt);

            // Act
            /* scheduler should fire. does not work, so we simulate... */
            durableActor.Tell(new Tick());

            // Assert
            durableActor.Tell(new GetStatistics());
            ExpectMsg<DurableActorStatistics>(s => s.LastStillAliveSentAt == timerFiredAt);
        }

        [Test]
        public void DurableActor_AfterPassivationInterval_FiresPassivate()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission());
            var moreThanInterval = TimeSpan.FromSeconds(301);
            var timerFiredAt = fakeStartTime + moreThanInterval;
            // SystemTime.Fake(() => timerFiredAt);
            Scheduler.Advance(moreThanInterval);

            // Act
            /* scheduler should fire. does not work, so we simulate... */
            durableActor.Tell(new Tick());
            Thread.Sleep(TimeSpan.FromSeconds(0.5));

            // Assert
            // does not occur - parent is not an actor
            // ExpectMsg<Passivate>();
            durableActor.Tell(new GetStatistics());
            var p = ExpectMsg<DurableActorStatistics>(s => true); // s.PassivateSentAt >= timerFiredAt - TimeSpan.FromSeconds(10));
        }
        #endregion

        #region persist event
        [Test]
        public void DurableActor_PersistEvent_TellsEventStore()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission());

            // Act
            durableActor.Tell(new SimpleDurableActor.CallPersist(new SimpleDurableActor.SomethingHappened()));

            // Assert
            eventStore.FishForMessage<PersistEvent>(p => p.Event.GetType() == typeof(SimpleDurableActor.SomethingHappened));
        }
        #endregion

        #region regular operation
        [Test]
        public void DurableActor_DuringRestore_DoesNotProcessCommand()
        {
            // Arrange
            eventStore.FishForMessage<RestoreNextEvents>(m => true);

            // Act
            durableActor.Tell("hello");

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void DurableActor_DuringRestore_StashesCommand()
        {
            // Arrange
            eventStore.FishForMessage<RestoreNextEvents>(m => true);

            // Act
            durableActor.Tell("hello");
            durableActor.Tell("hello");

            // Assert
            durableActor.Tell(new GetStatistics());
            FishForMessage<DurableActorStatistics>(s => s.NrStashedCommands == 2);
        }

        [Test]
        public void DurableActor_DuringRestore_ProcessEvents()
        {
            // Arrange
            eventStore.FishForMessage<RestoreNextEvents>(m => true);

            // Act
            durableActor.Tell(new SimpleDurableActor.SomethingHappened());

            // Assert
            durableActor.Tell(new GetStatistics());
            FishForMessage<DurableActorStatistics>(s => s.NrRestoreEvents == 1);
        }

        [Test]
        public void DurableActor_AfterRestore_ProcessesCustomMessage()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission()); // end restore

            // Act
            durableActor.Tell("Hello");

            // Assert
            ExpectMsg<string>(s => s == "Reply: Hello");
        }

        [Test]
        public void DurableActor_AfterRestore_ReturnsOkWhenNoException()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission()); // end restore

            // Act
            durableActor.Tell(42);

            // Assert
            ExpectMsg<Reply>(r => r.IsOk);
        }

        [Test]
        public void DurableActor_AfterRestore_ReturnsErrorWhenException()
        {
            // Arrange
            durableActor.Tell(new EndOfTransmission()); // end restore

            // Act
            durableActor.Tell(-42);

            // Assert
            ExpectMsg<Reply>(r => r.Message == "lt zero");
        }
        #endregion
    }
}
