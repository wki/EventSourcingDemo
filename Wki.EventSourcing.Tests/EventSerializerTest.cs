using NUnit.Framework;
using System;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Serialisation;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class EventSerializerTest
    {
        // Event with a full constructor
        public class TestEvent : Event<int?>
        {
            public int MyNumber { get; private set; }

            public TestEvent(DateTime occuredOn, int? id, int myNumber)
                : base(occuredOn, id)
            {
                MyNumber = myNumber;
            }
        }

        // Event with only a default constructor
        public class AnotherEvent : Event
        {
            public string MyText { get; private set; }

            public AnotherEvent(string myText)
            {
                MyText = myText;
            }
        }

        [Test]
        public void EventSerializer_SerializingEvent_AddsTypeInformation()
        {
            // Arrange
            var @event = new TestEvent(new DateTime(1964, 5, 3, /**/ 5, 32, 0), 77, 142);

            // Act
            var json = EventSerializer.ToJson(@event);
            Console.WriteLine("JSON: '{0}'", json);

            // Assert -- eigentlich ein implementierungs-detail.
            Assert.IsTrue(json.Contains(@"""$type"":"));
        }

        [Test]
        public void EventSerializer_DeserializeEvent_KeepsType()
        {
            // Arrange
            var day = new DateTime(1964, 3, 5, /**/ 19, 42, 0);
            var @event = new TestEvent(day, 88, 42);

            // Act
            var json = EventSerializer.ToJson(@event);
            Console.WriteLine("JSON: '{0}'", json);

            var restoredEvent = (TestEvent) EventSerializer.FromJson(json);

            // Assert
            Assert.AreSame(@event.GetType(), restoredEvent.GetType(), "Type");
            Assert.AreEqual(day, restoredEvent.OccuredOn, "OccuredOn");
            Assert.AreEqual(88, restoredEvent.Id, "Id");
            Assert.AreEqual(42, restoredEvent.MyNumber, "MyNumber");
        }

        [Test]
        public void EventSerializer_DeserializeEvent_UsesDefaultConstructorAndSetters()
        {
            // Arrange
            var day = new DateTime(2014, 1, 3, /**/ 3, 4, 15);
            SystemTime.Fake(() => day);
            var @event = new AnotherEvent("foo");

            // Act
            var json = EventSerializer.ToJson(@event);
            Console.WriteLine("JSON: '{0}'", json);

            SystemTime.Fake(() => DateTime.Now);
            var restoredEvent = (AnotherEvent)EventSerializer.FromJson(json);

            // Assert
            Assert.AreSame(@event.GetType(), restoredEvent.GetType(), "Type");
            Assert.AreEqual(day, restoredEvent.OccuredOn, "OccuredOn");
            Assert.AreEqual("foo", restoredEvent.MyText, "MyText");
        }
    }
}
