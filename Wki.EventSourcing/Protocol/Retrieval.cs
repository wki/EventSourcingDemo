using System;

namespace Wki.EventSourcing.Protocol.Load
{
    // Load Protocol
    //
    // LoadSnapshot(persistenceId) -> Snapshot / NoSnapshot
    // LoadNextEvents -> EventRecord* ... End
   
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
        public int StartAfterEventId { get; private set; }
        public int NrEvents { get; private set; }

        private LoadNextEvents(int startAfterEventId)
        {
            StartAfterEventId = startAfterEventId;
            NrEvents = 1000;
        }

        public static LoadNextEvents FromBeginning =>
            After(-1);

        public static LoadNextEvents After(int startAfterEventId) =>
            new LoadNextEvents(startAfterEventId);
    }

    /// <summary>
    /// End of transmission from LoadNextEvents
    /// </summary>
    public class End
    {
        private End _instance;

        public End Instance =>
            _instance ?? (_instance = new End());

        private End() {}
    }
}
