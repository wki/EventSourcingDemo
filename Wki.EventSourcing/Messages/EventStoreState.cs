using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Messages
{
    public enum EventStoreStatus
    {
        Initializing,
        Loading,
        Operating,
    }

    public class EventStoreState
    {
        // status
        public EventStoreStatus Status { get; internal set; }
        public DateTime StatusChangedAt { get; internal set; }

        // start information
        public DateTime StartedAt { get; internal set; }

        // load related things
        public TimeSpan LoadDuration { get; internal set; }
        public int NrEventsLoaded { get; internal set; }
        public int NrStashedCommands { get; internal set; }

        // information about durable actors served
        public int NrActorsRestored { get; internal set; }
        public int NrStillAliveReceived { get; internal set; }
        public int NrSubscribers { get; internal set; }

        // current size
        public DateTime LastEventPersistedAt { get; internal set; }
        public int NrEventsPersisted { get; internal set; }
        public int NrEventsTotal { get; internal set; }

        public EventStoreState()
        {
            var now = SystemTime.Now;

            Status = EventStoreStatus.Initializing;
            StatusChangedAt = now;
            StartedAt = now;
            LoadDuration = TimeSpan.FromSeconds(0);
            NrEventsLoaded = 0;
            NrStashedCommands = 0;
            NrActorsRestored = 0;
            NrStillAliveReceived = 0;
            NrSubscribers = 0;
            LastEventPersistedAt = DateTime.MinValue;
            NrEventsPersisted = 0;
            NrEventsTotal = 0;
        }

        internal void ChangeStatus(EventStoreStatus status)
        {
            Status = status;
            StatusChangedAt = SystemTime.Now;
        }

        public bool IsLoading() =>
            Status == EventStoreStatus.Loading;

        public bool IsOperating() =>
            Status == EventStoreStatus.Operating;
    }
}
