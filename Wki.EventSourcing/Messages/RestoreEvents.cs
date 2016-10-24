using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Command to EventStore to expect the next n events
    /// </summary>
    public class RestoreEvents
    {
        private const int DefaultNrEvents = 1000;

        /// <summary>
        /// Number of events to restore as one junk
        /// </summary>
        /// <value>The nr of events.</value>
        public int NrEvents { get; private set; }

        public RestoreEvents() : this(DefaultNrEvents) { }

        public RestoreEvents(int nrEvents)
        {
            NrEvents = nrEvents;
        }
    }
}
