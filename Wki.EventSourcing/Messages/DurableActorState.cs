using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Messages
{
    public enum DurableActorStatus
    {
        Initializing,
        Restoring,
        Operating,
        //Idle,
        //Stopping
    }

    /// <summary>
    /// Document Message (and internal storage) for the status of a durable actor
    /// </summary>
    public class DurableActorState
    {
        // status
        public DurableActorStatus Status { get; internal set; }
        public DateTime StatusChangedAt { get; internal set; }
        public bool IsRestoring { get { return Status == DurableActorStatus.Restoring; } }
        public bool IsOperating { get { return Status == DurableActorStatus.Operating; } }

        // start information
        public DateTime StartedAt { get; internal set; }

        // restore related things
        public TimeSpan RestoreDuration { get; internal set; }
        public int NrRestoreEvents { get; internal set; }
        public int NrStashedCommands { get; internal set; }

        // various runtime statistics
        public DateTime LastCommandReceivedAt { get; internal set; }
        public DateTime LastEventReceivedAt { get; internal set; }
        public int NrEventsTotal { get; internal set; }
        public int NrCommandsTotal { get; internal set; }
        public int NrUnhandledMessages { get; internal set; }

        // stillalive send time
        public DateTime LastStillAliveSentAt { get; internal set; }

        public DurableActorState()
        {
            var now = SystemTime.Now;

            Status = DurableActorStatus.Initializing;
            StatusChangedAt = now;
            StartedAt = now;
            RestoreDuration = TimeSpan.FromSeconds(0);
            NrRestoreEvents = 0;
            NrStashedCommands = 0;
            LastCommandReceivedAt = DateTime.MinValue;
            LastEventReceivedAt = DateTime.MinValue;
            NrEventsTotal = 0;
            NrCommandsTotal = 0;
            NrUnhandledMessages = 0;
            LastStillAliveSentAt = DateTime.MinValue;
        }

        internal void ChangeStatus(DurableActorStatus status)
        {
            Status = status;
            StatusChangedAt = SystemTime.Now;
        }
    }
}
