using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Command to EventStore: start restoring a given durable actor
    /// </summary>
    public class StartRestore
    {
        public InterestingEvents InterestingEvents { get; private set; }

        public StartRestore(InterestingEvents interestingEvents)
        {
            InterestingEvents = interestingEvents;
        }
    }
}
