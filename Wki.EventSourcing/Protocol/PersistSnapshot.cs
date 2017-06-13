namespace Wki.EventSourcing.Protocol
{
    // sent to EventStore to create a snapshot
    
    public class PersistSnapshot
    {
        public State State { get; private set; }

        public string PersistenceId { get; private set; }

        public int LastEventId { get; private set; }

        public PersistSnapshot(State state, string persistenceId, int lastEventId)
        {
            State = state;
            PersistenceId = persistenceId;
            LastEventId = lastEventId;
        }
    }
}