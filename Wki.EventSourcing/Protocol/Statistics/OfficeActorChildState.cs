using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol.Statistics
{
    public class OfficeActorChildState
    {
        // start information
        public DateTime StartedAt { get; internal set; }

        // alive management
        public DateTime LastStatusQuerySentAt { get; internal set; }
        public DateTime LastStatusReceivedAt { get; internal set; }
        public DurableActorStatus Status { get; internal set; }

        // command forwarding
        public int NrCommandsForwarded { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public OfficeActorChildState()
        {
            var now = SystemTime.Now;
            StartedAt = now;
            LastStatusQuerySentAt = DateTime.MinValue;
            LastStatusReceivedAt = DateTime.MinValue;
            Status = DurableActorStatus.Initializing;
            NrCommandsForwarded = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }
    }
}
