using Akka.Actor;
using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// current state of a child actor from an office's view
    /// </summary>
    public class OfficeActorChildState
    {
        // the actor
        public IActorRef Child { get; internal set; }

        // start information
        public DateTime StartedAt { get; internal set; }

        // current status
        public OfficeActorChildStatus Status { get; internal set; }
        public DateTime StatusChangedAt { get; set; }

        // alive management
        public DateTime LastStillAliveReceivedAt { get; internal set; }

        // command forwarding
        public int NrCommandsForwarded { get; internal set; }
        public DateTime LastCommandForwardedAt { get; internal set; }

        public OfficeActorChildState(IActorRef child)
        {
            Child = child;

            var now = SystemTime.Now;

            StartedAt = now;

            Status = OfficeActorChildStatus.Operating;
            StatusChangedAt = now;

            LastStillAliveReceivedAt = DateTime.MinValue;

            NrCommandsForwarded = 0;
            LastCommandForwardedAt = DateTime.MinValue;
        }

        public void StillAlive()
        {
            LastStillAliveReceivedAt = SystemTime.Now;
            ChangeStatus(OfficeActorChildStatus.Operating);
        }

        public void CommandForwarded()
        {
            NrCommandsForwarded++;
            LastCommandForwardedAt = SystemTime.Now;
        }

        public void ChangeStatus(OfficeActorChildStatus status)
        {
            if (Status != status)
            {
                StatusChangedAt = SystemTime.Now;
                Status = status;
            }
        }
    }
}
