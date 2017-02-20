    using System;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// Document Message (and internal storage) with statistics for a durable actor
    /// </summary>
    public class DurableActorStatistics
    {
        // status
        public DurableActorStatus Status { get; private set; }
        public DateTime StatusChangedAt { get; private set; }
        public bool IsRestoring { get { return Status == DurableActorStatus.Restoring; } }
        public bool IsOperating { get { return Status == DurableActorStatus.Operating; } }

        // start information
        public DateTime StartedAt { get; private set; }

        // restore related things
        public TimeSpan RestoreDuration { get; private set; }
        public int NrRestoreEvents { get; private set; }
        public int NrStashedCommands { get; private set; }

        // various runtime statistics
        public DateTime LastCommandReceivedAt { get; private set; }
        public DateTime LastEventReceivedAt { get; private set; }
        public int NrEventsTotal { get; private set; }
        public int NrCommandsTotal { get; private set; }
        public int NrUnhandledMessages { get; private set; }

        // last time something happened (except keepalive)
        public DateTime LastActivity { get { return LastCommandReceivedAt > LastEventReceivedAt ? LastCommandReceivedAt : LastEventReceivedAt; } }

        // stillalive send time
        public DateTime LastStillAliveSentAt { get; private set; }

        public DurableActorStatistics()
        {
            DateTime startTime = SystemTime.Now;

            Status = DurableActorStatus.Initializing;
            StatusChangedAt = startTime;
            StartedAt = startTime;
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
            if (Status == DurableActorStatus.Restoring)
            {
                RestoreDuration = SystemTime.Now - StartedAt;
            }
            Status = status;
            StatusChangedAt = SystemTime.Now;
        }

        internal void CommandReceived()
        {
            if (Status == DurableActorStatus.Restoring)
                NrStashedCommands++;
            else
                NrCommandsTotal++;
            LastCommandReceivedAt = SystemTime.Now;
        }

        internal void EventReceived()
        {
            if (Status == DurableActorStatus.Restoring)
                NrRestoreEvents++;
            else
                NrEventsTotal++;
            LastEventReceivedAt = SystemTime.Now;
        }

        internal void UnhandledMessageReceived()
        {
            NrUnhandledMessages++;
        }

        internal void StillAliveSent()
        {
            LastStillAliveSentAt = SystemTime.Now;
        }

        internal bool ShouldPassivate()
        {
            return SystemTime.Now > LastActivity + MaxActorIdleTimeSpan;
        }
    }
}
