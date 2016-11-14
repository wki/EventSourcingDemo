using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Messages
{
    public class OfficeActorState
    {
        // start information
        public DateTime StartedAt { get; internal set; }

        // durable actors
        public int NrActorsLoaded { get; internal set; }
        public DateTime LastActorLoadedAt { get; internal set; }
        public int NrActorsRemoved { get; internal set; }
        public DateTime LastActorRemovedAt { get; internal set; }

        // message handling
        public int NrCommandsForwarded { get; internal set; }
        public int NrUnhandledMessages { get; internal set; }

        public OfficeActorState()
        {
            var now = SystemTime.Now;
            StartedAt = now;
            NrActorsLoaded = 0;
            NrActorsRemoved = 0;
            NrCommandsForwarded = 0;
            NrUnhandledMessages = 0;
        }
    }
}
