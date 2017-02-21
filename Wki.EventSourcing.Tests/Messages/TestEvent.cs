using System;
using Wki.EventSourcing.Protocol;

namespace Wki.EventSourcing.Tests.Messages
{
    /// <summary>
    /// Event with a full constructor
    /// </summary>
    public class TestEvent : Event<int?>
    {
        public int MyNumber { get; private set; }

        public TestEvent(DateTime occuredOn, int? id, int myNumber)
            : base(occuredOn, id)
        {
            MyNumber = myNumber;
        }
    }
}
