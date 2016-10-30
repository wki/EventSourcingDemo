using System;
using NUnit.Framework;
using Wki.EventSourcing.Messages;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class EventTest
    {
        public class EventA : Event { }
        public class EventB : Event<int>
        {
            public EventB(int id) : base(id) {}
        }

        [Test]
        public void EventWithoutId_GetId_ReturnsNull()
        {
            // Arrange
            var @event = new EventA();

            // Assert
            Assert.IsNull(@event.GetId());
        }

        [Test]
        public void EventWithId_GetId_ReturnsStringifiedId()
        {
            // Arrange
            var @event = new EventB(77);

            // Assert
            Assert.AreEqual("77", @event.GetId());
        }
    }
}
