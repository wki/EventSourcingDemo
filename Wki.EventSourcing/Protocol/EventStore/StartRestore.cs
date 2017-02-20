using System;
using Wki.EventSourcing.Protocol.Subscription;

namespace Wki.EventSourcing.Protocol.EventStore
{
    /// <summary>
    /// Command to EventStore: start restoring a given durable actor
    /// </summary>
    public class StartRestore
    {
        public InterestingEvents InterestingEvents { get; private set; }

        public StartRestore(InterestingEvents interestingEvents)
        {
            if (interestingEvents == null)
                throw new ArgumentNullException(nameof(interestingEvents));

            InterestingEvents = interestingEvents;
        }
    }
}
