using System;
using System.Collections.Generic;
using System.Linq;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// Statistics for an office
    /// </summary>
    public class OfficeActorStatistics
    {
        // start information
        public DateTime StartedAt { get; internal set; }

        // durable actors
        public int NrActorsLoaded { get; internal set; }
        public DateTime LastActorLoadedAt { get; internal set; }
        public int NrActorsRemoved { get; internal set; }
        public DateTime LastActorRemovedAt { get; internal set; }
        public int NrActorsDied { get; internal set; }
        public DateTime LastActorDiedAt { get; internal set; }
        public int NrActorsDiedDuringRestore { get; internal set; }
        public DateTime LastActorDiedDuringRestoreAt { get; set; }

        // message handling
        public int NrCommandsForwarded { get; internal set; }
        public int NrUnhandledMessages { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public OfficeActorStatistics()
        {
            var now = SystemTime.Now;

            StartedAt = now;

            NrActorsLoaded = 0;
            LastActorLoadedAt = DateTime.MinValue;
            NrActorsRemoved = 0;
            LastActorRemovedAt = DateTime.MinValue;
            NrActorsDied = 0;
            LastActorDiedAt = DateTime.MinValue;
            NrActorsDiedDuringRestore = 0;
            LastActorDiedDuringRestoreAt = DateTime.MinValue;

            NrCommandsForwarded = 0;
            NrUnhandledMessages = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }

        internal void ActorAdded()
        {
            NrActorsLoaded++;
            LastActorLoadedAt = SystemTime.Now;
        }

        internal void ActorRemoved()
        {
            NrActorsRemoved++;
            LastActorRemovedAt = SystemTime.Now;
        }

        internal void ActorDied()
        {
            NrActorsDied++;
            LastActorDiedAt = SystemTime.Now;
        }

        internal void ActorDiedDuringRestore()
        {
            NrActorsDiedDuringRestore++;
            LastActorDiedDuringRestoreAt = SystemTime.Now;
        }

        internal void ForwardedCommand()
        {
            NrCommandsForwarded++;
            LastCommandForwardedAt = SystemTime.Now;
        }

        internal void UnhandledMessage()
        {
            NrUnhandledMessages++;
        }
    }
}
