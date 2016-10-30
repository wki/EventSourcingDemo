﻿using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Akka.Actor;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;
using static Wki.EventSourcing.Tests.TempDir;

namespace Wki.EventSourcing.Tests
{
    public class IgnoreWhatHappened : Event { }

    public class SomethingHappened : Event 
    {
        public int Number { get; private set; }

        public SomethingHappened(int number = 0)
        {
            Number = number;
        }
    }

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
            reader.ExpectMsgFrom<LoadJournal>(eventStore, loadJournal => loadJournal.NrEvents == NrRestoreEvents);
        }
        #endregion

        #region load journal behavior
        [Test]
        public void EventStore_AfterReceiving90Events_Requests100More()
        {
            // Assert
            reader.ExpectMsgFrom<LoadJournal>(eventStore, load => load.NrEvents == NrRestoreEvents);
            for (var i = 1; i <= 90; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened()));
            reader.ExpectMsgFrom<LoadJournal>(eventStore, load => load.NrEvents == NrRestoreEvents);
        }

        [Test]
        public void EventStore_AfterLoading_HasEmptyEventList()
        {
            // Act
            eventStore.Tell(new End());

            // Assert
            eventStore.Tell(new GetSize());
            ExpectMsg<int>(0);
        }

        [Test]
        public void EventStore_AfterLoadingEvents_HasFilledEventList()
        {
            // Act
            for (var i = 1; i <= 10; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new End());

            // Assert
            eventStore.Tell(new GetSize());
            ExpectMsg<int>(10);
        }
        #endregion

        #region restore event behavior
        [Test]
        public void EventStore_RestoreEvents_PlaysBackEvents()
        {
            // Arrange -- load 11 Events
            eventStore.Tell(new EventLoaded(new IgnoreWhatHappened()));
            for (var i = 1; i <= 10; i++)
                eventStore.Tell(new EventLoaded(new SomethingHappened(i)));
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new RestoreEvents(3));

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
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new RestoreEvents(3));

            // Assert
            ExpectMsg<SomethingHappened>(something => something.Number == 1);
            ExpectMsg<SomethingHappened>(something => something.Number == 2);
            ExpectMsg<SomethingHappened>(something => something.Number == 3);

            eventStore.Tell(new RestoreEvents(2));
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
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new RestoreEvents(999));

            // Assert
            ExpectMsg<SomethingHappened>(something => something.Number == 1);
            ExpectMsg<SomethingHappened>(something => something.Number == 2);
            ExpectMsg<SomethingHappened>(something => something.Number == 3);
            ExpectMsg<SomethingHappened>(something => something.Number == 4);

            ExpectMsg<End>();
        }

        [Test]
        public void EventStore_AfterStartRestore_ReportsActorRestoring()
        {
            // Arrange
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new GetActors());

            // Assert
            ExpectMsg<string>(s => Regex.IsMatch(s, ">akka://test/user/testActor.*-0"));
        }

        [Test]
        public void EventStore_AfterEndRestore_ReportsActorRunning()
        {
            // Arrange
            var now = new DateTime(2010, 3, 5, 20, 0, 0);
            var now3 = now.AddSeconds(3);

            SystemTime.Fake(() => now);
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));
            eventStore.Tell(new RestoreEvents(999));

            // Assert
            ExpectMsg<End>();

            SystemTime.Fake(() => now3);
            eventStore.Tell(new GetActors());

            ExpectMsg<string>(s => Regex.IsMatch(s, "=akka://test/user/testActor.*-3"));
        }
        #endregion

        #region actor keepalive behavior
        [Test]
        public void EventStore_AfterLongIdleTime_RemovesActor()
        {
            // Arrange
            var now = new DateTime(2010, 3, 5, 20, 0, 0);
            var now6 = now.AddMinutes(6); // idle time is 5 minutes

            SystemTime.Fake(() => now);
            eventStore.Tell(new End());

            // Act
            eventStore.Tell(new StartRestore(new InterestingEvents(new[] { typeof(SomethingHappened) })));

            // Assert
            eventStore.Tell(new GetActors());
            ExpectMsg<string>();

            SystemTime.Fake(() => now6);

            eventStore.Tell(new RemoveLostActors());
            eventStore.Tell(new GetActors());

            ExpectMsg<string>(s => String.IsNullOrEmpty(s));
        }
        #endregion
    }
}
