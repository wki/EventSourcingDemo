using System;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Tests.Messages
{
    public class SomethingHappened : Event
    {
        public int Number { get; private set; }

        public SomethingHappened() : this(SystemTime.Now, 0) { }

        public SomethingHappened(int number) : this(SystemTime.Now, number) { }

        public SomethingHappened(DateTime occuredOn, int number)
            : base(occuredOn)
        {
            Number = number;
        }
    }
}
