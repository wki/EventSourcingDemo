using System;
using System.Collections.Generic;

namespace Wki.EventSourcing.Protocol.Statistics
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

        public EventStoreStatistics EventStoreStatistics { get; internal set; }

        public List<ActorStatus> Actors { get; internal set; }
    }
}
