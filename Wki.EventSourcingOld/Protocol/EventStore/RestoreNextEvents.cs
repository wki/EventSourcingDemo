using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Protocol.EventStore
{
    /// <summary>
    /// Command to EventStore to expect the next n events
    /// </summary>
    public class RestoreNextEvents
    {
        /// <summary>
        /// Number of events to restore as one junk
        /// </summary>
        /// <value>The nr of events.</value>
        public int NrEvents { get; private set; }

        public RestoreNextEvents() : this(DefaultNrEvents) { }

        public RestoreNextEvents(int nrEvents)
        {
            NrEvents = nrEvents;
        }
    }
}
