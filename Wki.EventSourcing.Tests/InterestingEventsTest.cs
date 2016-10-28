using System;
using Wki.EventSourcing.Messages;
using NUnit.Framework;
using System.Threading;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class InterestingEventsTest
    {
        private class GoodEvent : Event<string> 
        {
            public GoodEvent(string id) : base(id) {}
        }

        private class AnotherEvent : Event<string> 
        {
            public AnotherEvent(string id) : base(id) {}
        }

        private class MoreEvent : Event<string> 
        {
            public MoreEvent(string id) : base(id) {}
        }

        private class BadEvent : Event<int?> 
        {
            public BadEvent(int? id) : base(id) {}
        }

        private InterestingEvents<string> interestingSpecificEvents;
        private InterestingEvents interestingAllEvents;

        [SetUp]
        public void Setup()
        {
            interestingSpecificEvents = new InterestingEvents<string>(
                "foo",
                new[] { typeof(GoodEvent), typeof(AnotherEvent) }
            );

            interestingAllEvents = new InterestingEvents(
                new[] { typeof(GoodEvent), typeof(AnotherEvent) }
            );
        }

        [Test]
        public void MatchingSpecificEvent_EventWithDifferentIndexType_DoesNotMatch()
        {
            // Arrange
            var @event = new BadEvent(42);

            // Assert
            Assert.IsFalse(interestingSpecificEvents.Matches(@event));
        }

        [Test]
        public void MatchingSpecificEvent_EventWithDifferentIndex_DoesNotMatch()
        {
            // Arrange
            var @event = new GoodEvent("bar");

            // Assert
            Assert.IsFalse(interestingSpecificEvents.Matches(@event));
        }

        [Test]
        public void MatchingSpecificEvent_UnknownEventType_DoesNotMatch()
        {
            // Arrange
            var @event = new MoreEvent("foo");

            // Assert
            Assert.IsFalse(interestingSpecificEvents.Matches(@event));
        }

        [Test]
        public void MatchingSpecificEvent_EventWithSameIndex_DoesMatch()
        {
            // Arrange
            var @event = new GoodEvent("foo");

            // Assert
            Assert.IsTrue(interestingSpecificEvents.Matches(@event));
        }

        [Test]
        public void MatchingAllEvents_EventWithKnownType_DoesMatch()
        {
            // Arrange
            var @event = new GoodEvent("xxx");
            Thread.Sleep(TimeSpan.FromSeconds(1));

            // Assert
            Assert.IsTrue(interestingAllEvents.Matches(@event));
        }
    }
}
