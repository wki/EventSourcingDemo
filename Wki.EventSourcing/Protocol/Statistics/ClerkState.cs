using Akka.Actor;
using System;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// current state of a child actor from an office's view
    /// </summary>
    public class ClerkState
    {
        // the actor
        public IActorRef Clerk { get; internal set; }

        // start information
        public DateTime StartedAt { get; internal set; }

        // current status
        public ClerkStatus Status { get; internal set; }
        public DateTime StatusChangedAt { get; set; }

        // alive management
        public int NrStillAliveReceived { get; set; }
        public DateTime LastStillAliveReceivedAt { get; internal set; }

        // command forwarding
        public int NrCommandsForwarded { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public ClerkState(IActorRef clerk)
        {
            Clerk = clerk;

            var now = SystemTime.Now;

            StartedAt = now;

            Status = ClerkStatus.Operating;
            StatusChangedAt = now;

            NrStillAliveReceived = 0;
            LastStillAliveReceivedAt = DateTime.MinValue;

            NrCommandsForwarded = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }

        public bool IsOperating() => 
            Status == ClerkStatus.Operating;

        public bool IsDead() =>
            IsOperating() &&
            SystemTime.Now > LastStillAliveReceivedAt + DeadActorRemoveTimeSpan;

        public void StillAlive()
        {
            NrStillAliveReceived++;
            LastStillAliveReceivedAt = SystemTime.Now;

            // just in case we some day have a "maybe dead" state...
            ChangeStatus(ClerkStatus.Operating);
        }

        public void CommandForwarded()
        {
            NrCommandsForwarded++;
            LastCommandForwardedAt = SystemTime.Now;
        }

        public void ChangeStatus(ClerkStatus status)
        {
            if (Status != status)
            {
                StatusChangedAt = SystemTime.Now;
                Status = status;
            }
        }
    }
}
