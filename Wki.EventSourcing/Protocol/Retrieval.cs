using System;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Protocol.Retrieval
{
    // Retrieval Protocol
   
    /// <summary>
    /// Requests a snapshot for a given PersistenceId to be loaded if present
    /// </summary>
    public class LoadSnapshot
    {
        public string PersistenceId { get; private set; }

        public Type StateType { get; private set; }

        public LoadSnapshot(string persistenceId, Type stateType)
        {
            PersistenceId = persistenceId;
            StateType = stateType;
        }
    }

    /// <summary>
    /// Response from Journal: no snapshot available
    /// </summary>
    public class NoSnapshot
    {
        private static NoSnapshot _instance;

        public static NoSnapshot Instance =>
            _instance ?? (_instance = new NoSnapshot());


        private NoSnapshot() {}
    }

    /// <summary>
    /// Response from Journal containing a snapshot
    /// </summary>
    public class Snapshot
    {
        public object State { get; private set; }

        public int LastEventId { get; private set; }

        public Snapshot(object state, int lastEventId)
        {
            State = state;
            LastEventId = lastEventId;
        }
    }

    /// <summary>
    /// Request Events (as EventRecord) from EventStore or Journal starting at a given position
    /// </summary>
    public class LoadNextEvents
    {
        public EventFilter EventFilter { get; private set; }
        public int NrEvents { get; private set; }

        private LoadNextEvents(EventFilter eventFilter)
        {
            EventFilter = eventFilter;
            NrEvents = DefaultNrEvents;
        }

        public static LoadNextEvents FromBeginning() =>
            new LoadNextEvents(WantEvents.FromBeginning());

        public static LoadNextEvents After(int startAfterEventId) =>
            new LoadNextEvents(WantEvents.After(startAfterEventId));

        public LoadNextEvents Load(int nrEvents)
        {
            NrEvents = nrEvents;
            return this;
        }
    }

    /// <summary>
    /// End of transmission from LoadNextEvents
    /// </summary>
    public class End
    {
        private static End _instance;

        public static End Instance =>
            _instance ?? (_instance = new End());

        private End() {}
    }
}
