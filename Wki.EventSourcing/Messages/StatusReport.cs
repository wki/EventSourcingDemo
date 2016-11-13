using System;
using System.Collections.Generic;

namespace Wki.EventSourcing.Messages
{
    public class StatusReport
    {
        public class ActorStatus
        {
            public string Path { get; internal set; }
            public string Status { get; internal set; }
            public DateTime LastSeen { get; internal set; }
            public List<string> Events { get; internal set; }

        }

        public DateTime StartedAt { get; internal set; }
        public int LoadDurationMilliseconds { get; internal set; }
        // "Loading" / "Operating"
        public string Status { get; internal set; }
        public int EventStoreSize { get; internal set; }

        public List<ActorStatus> Actors { get; internal set; }
    }
}
