using System;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// Document Message (and internal storage) with statistics for the event store
    /// </summary>
    public class EventStoreStatistics
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
        public int NrSubscribers { get; internal set; }

        // current size
        public DateTime LastEventPersistedAt { get; internal set; }
        public int NrEventsPersisted { get; internal set; }
        public int NrEventsTotal { get; internal set; }

        public EventStoreStatistics()
        {
            var now = SystemTime.Now;

            Status = EventStoreStatus.Initializing;
            StatusChangedAt = now;
            StartedAt = now;
            LoadDuration = TimeSpan.FromSeconds(0);
            NrEventsLoaded = 0;
            NrStashedCommands = 0;
            NrActorsRestored = 0;
            NrSubscribers = 0;
            LastEventPersistedAt = DateTime.MinValue;
            NrEventsPersisted = 0;
            NrEventsTotal = 0;
        }

        /// <summary>
        /// Change the status of the event store
        /// </summary>
        /// <param name="status"></param>
        internal void ChangeStatus(EventStoreStatus status)
        {
            Status = status;
            StatusChangedAt = SystemTime.Now;

            if (status == EventStoreStatus.Operating)
                LoadDuration = SystemTime.Now - StartedAt; 
        }

        /// <summary>
        /// update statistics after a stashed command
        /// </summary>
        internal void StashedCommand()
        {
            NrStashedCommands++;
        }

        /// <summary>
        /// update statistics after an event was loaded
        /// </summary>
        internal void LoadedEvent()
        {
            NrEventsLoaded++;
            NrEventsTotal++;
        }

        /// <summary>
        /// update statistics when starting to restore an actor
        /// </summary>
        internal void StartRestore()
        {
            NrActorsRestored++;
        }

        /// <summary>
        /// update statistics when persisting an event
        /// </summary>
        internal void PersistedEvent()
        {
            NrEventsPersisted++;
            NrEventsTotal++;
            LastEventPersistedAt = SystemTime.Now;
        }

        /// <summary>
        /// report if we are loading
        /// </summary>
        /// <returns></returns>
        public bool IsLoading() =>
            Status == EventStoreStatus.Loading;

        /// <summary>
        /// report if we are operating
        /// </summary>
        /// <returns></returns>
        public bool IsOperating() =>
            Status == EventStoreStatus.Operating;
    }
}
