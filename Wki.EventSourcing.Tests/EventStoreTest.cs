using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Akka.Actor;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using static Wki.EventSourcing.Tests.TempDir;
using Wki.EventSourcing.Tests.Messages;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.Subscription;
using Wki.EventSourcing.Protocol.LiveCycle;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wki.EventSourcing.Tests
{
    public class IgnoreWhatHappened : Event { }


    [TestFixture]
    public class EventStoreTest : TestKit
    {
        private static string storageDir;
        private IActorRef eventStore;
        private TestProbe reader;
        private TestProbe writer;

        [TestFixtureSetUp]
        public static void TestFuxtureSetUp()
        {
            storageDir = CreateTempDir();
        }

        [SetUp]
        public void SetUp()
        {
            reader = CreateTestProbe("reader");
            writer = CreateTestProbe("writer");
            eventStore = Sys.ActorOf(Props.Create<EventStore>(storageDir, reader, writer), "eventstore");
        }

        // [TearDown]
        // public void TearDown()
        // {
        // }

        [TestFixtureTearDown]
        public static void TestFixtureTearDown()
        {
            RemoveTempDir(storageDir);
        }

        #region start behaviour
        [Test]
        public void EventStore_AfterStart_LoadsJournal()
        {
            // Assert
            reader.ExpectMsgFrom<LoadNextEvents>(eventStore, loadJournal => loadJournal.NrEvents == NrRestoreEvents);
        }

        [Test]
        public void EventStore_AfterStart_HasEmptyStatusReport()
        {
            // Act
            eventStore.Tell(new GetStatistics());

            // Assert
            FishForMessage<EventStoreStatistics>(s => s.NrSubscribers == 0); 
        }
        #endregion

        #region protocol: load
        [Test]
        public void EventStore_BeforeReachingBufferLowLimit_DoesNotRequestMore()
        {
            // Assert
            reader.ExpectMsgFrom<LoadNextEvents>(eventStore, load => load.NrEvents == NrRestoreEvents);
            for (var i = 1; i < NrRestoreEvents - BufferLowLimit; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened()));
            reader.ExpectNoMsg(TimeSpan.FromSeconds(0.1));
        }

        [Test]
        public void EventStore_AfterReachingBufferLowLimit_RequestsMore()
        {
            // Assert
            reader.ExpectMsgFrom<LoadNextEvents>(eventStore, load => load.NrEvents == NrRestoreEvents);
            for (var i = 1; i <= NrRestoreEvents - BufferLowLimit; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened()));
            reader.ExpectMsgFrom<LoadNextEvents>(eventStore, load => load.NrEvents == NrRestoreEvents);
        }

        [Test]
        public void EventStore_AfterLoading_HasEmptyEventList()
        {
            // Act
            reader.ExpectMsgFrom<LoadNextEvents>(eventStore, load => load.NrEvents == NrRestoreEvents);
            eventStore.Tell(new EndOfTransmission());
            reader.ExpectNoMsg(TimeSpan.FromSeconds(0.1));

            // Assert
            //eventStore.Tell(new GetSize());
            //ExpectMsg<int>(0);
            eventStore.Tell(new GetStatistics());
            ExpectMsg<EventStoreStatistics>(s => s.NrEventsLoaded == 0);
        }

        [Test]
        public void EventStore_AfterLoadingEvents_HasFilledEventList()
        {
            // Act
            for (var i = 1; i <= 10; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new EndOfTransmission());

            // Assert
            //eventStore.Tell(new GetSize());
            //ExpectMsg<int>(10);
            eventStore.Tell(new GetStatistics());
            ExpectMsg<EventStoreStatistics>(s => s.NrEventsLoaded == 10);
        }
        #endregion

        #region protocol: subscribe

        #endregion

        #region protocol: reconstitute
        [Test]
        public void EventStore_RestoreEvents_PlaysBackEvents()
        {
            // Arrange -- load 11 Events
            eventStore.Tell(new EventLoaded(new IgnoreWhatHappened()));
            for (var i = 1; i <= 10; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new EndOfTransmission());

            // Act
            eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new StartRestore());
            eventStore.Tell(new RestoreNextEvents(3));

            // Assert
            ExpectMsg<SomethingHappened>(something => something.Number == 1);
            ExpectMsg<SomethingHappened>(something => something.Number == 2);
            ExpectMsg<SomethingHappened>(something => something.Number == 3);

            ExpectNoMsg(TimeSpan.FromSeconds(0.1));
        }

        [Test]
        public void EventStore_RestoreEventsInJunks_PlaysBackEventJunks()
        {
            // Arrange -- load 11 Events
            eventStore.Tell(new EventLoaded(new IgnoreWhatHappened()));
            for (var i = 1; i <= 10; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new EndOfTransmission());

            // Act
            eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new StartRestore());
            eventStore.Tell(new RestoreNextEvents(3));

            // Assert
            ExpectMsg<SomethingHappened>(something => something.Number == 1);
            ExpectMsg<SomethingHappened>(something => something.Number == 2);
            ExpectMsg<SomethingHappened>(something => something.Number == 3);

            eventStore.Tell(new RestoreNextEvents(2));
            ExpectMsg<SomethingHappened>(something => something.Number == 4);
            ExpectMsg<SomethingHappened>(something => something.Number == 5);
        }

        [Test]
        public void EventStore_RestoreEventsCompletely_PlaysBackAllEvents()
        {
            // Arrange -- load 11 Events
            eventStore.Tell(new EventLoaded(new IgnoreWhatHappened()));
            for (var i = 1; i <= 4; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new EndOfTransmission());

            // Act
            eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new StartRestore());
            eventStore.Tell(new RestoreNextEvents(999));

            // Assert
            ExpectMsg<SomethingHappened>(something => something.Number == 1);
            ExpectMsg<SomethingHappened>(something => something.Number == 2);
            ExpectMsg<SomethingHappened>(something => something.Number == 3);
            ExpectMsg<SomethingHappened>(something => something.Number == 4);

            ExpectMsg<EndOfTransmission>();
        }
        
        [Test]
        public void EventStore_AfterStartRestore_ReportsActorRestoring()
        {
            // Arrange
            eventStore.Tell(new EndOfTransmission());

            // Act
            eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new StartRestore());
            eventStore.Tell(new GetStatistics());

            // Assert
            // TODO: ist das korrekt?
            ExpectMsg<EventStoreStatistics>(s => s.NrActorsRestored == 1);
        }

        [Test]
        public void EventStore_AfterEndRestore_ReportsActorRunning()
        {
            // Arrange
            var now = new DateTime(2010, 3, 5, 20, 0, 0);
            var now3 = now.AddSeconds(3);

            SystemTime.Fake(() => now);
            eventStore.Tell(new EndOfTransmission());

            // Act
            eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new StartRestore());
            eventStore.Tell(new RestoreNextEvents(999));

            // Assert
            ExpectMsg<EndOfTransmission>();

            SystemTime.Fake(() => now3);
            eventStore.Tell(new GetStatistics());

            // TODO: ist das korrekt?
            ExpectMsg<EventStoreStatistics>(s => s.NrSubscribers == 1);
        }
        #endregion

        #region actor keepalive behavior

        // TODO: keepalive monitoring
        // TODO: passivation

        // [Test]
        //public void EventStore_AfterLongIdleTime_RemovesActor()
        //{
        //    // Arrange
        //    var now = new DateTime(2010, 3, 5, 20, 0, 0);
        //    var now6 = now.AddMinutes(6); // idle time is 5 minutes

        //    SystemTime.Fake(() => now);
        //    eventStore.Tell(new End());

        //    // Act
        //    eventStore.Tell(new Subscribe(new InterestingEvents(new[] { typeof(SomethingHappened) })));
        //    eventStore.Tell(new StartRestore());

        //    // Assert
        //    eventStore.Tell(new GetActors());
        //    ExpectMsg<string>();

        //    SystemTime.Fake(() => now6);

        //    eventStore.Tell(new RemoveInactiveActors());
        //    eventStore.Tell(new GetActors());

        //    ExpectMsg<string>(s => String.IsNullOrEmpty(s));
        //}
        #endregion
    }
}
