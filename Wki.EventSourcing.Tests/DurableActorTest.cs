﻿using System;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Tests
{
    public class SimpleDurableActor : DurableActor
    {
        public SimpleDurableActor(IActorRef eventStore) : base(eventStore)
        {
            Receive<string>(s => Sender.Tell($"Reply: {s}"));
            Receive<int>(i => { if (i < 0) throw new ArgumentException("lt zero"); });
        }
    }

    [TestFixture]
    public class DurableActorTest : TestKit
    {
        private TestProbe eventStore;
        private IActorRef durableActor;

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("eventStore");
            durableActor = Sys.ActorOf(Props.Create<SimpleDurableActor>(eventStore));
        }

        [Test]
        public void DurableActor_AfterStart_RestoresEvents()
        {
            // Assert
            eventStore.ExpectMsg<StartRestore>();
            eventStore.ExpectMsg<RestoreEvents>(r => r.NrEvents == NrRestoreEvents);
        }

        [Test]
        public void DurableActor_AfterStart_DoesNotProcessCustomMessage()
        {
            // Act
            durableActor.Tell("Hello");

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.1));
        }

        [Test]
        public void DurableActor_AfterRestore_ProcessesCustomMessage()
        {
            // Arrange
            durableActor.Tell(new End()); // end restore

            // Act
            durableActor.Tell("Hello");

            // Assert
            ExpectMsg<string>(s => s == "Reply: Hello");
        }

        [Test]
        public void DurableActor_AfterRestore_ReturnsOkWhenNoException()
        {
            // Arrange
            durableActor.Tell(new End()); // end restore

            // Act
            durableActor.Tell(42);

            // Assert
            ExpectMsg<Reply>(r => r.IsOk);
        }

        [Test]
        public void DurableActor_AfterRestore_ReturnsErrorWhenException()
        {
            // Arrange
            durableActor.Tell(new End()); // end restore

            // Act
            durableActor.Tell(-42);

            // Assert
            ExpectMsg<Reply>(r => r.Message == "lt zero");
        }

    }
}