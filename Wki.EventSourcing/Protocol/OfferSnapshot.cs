namespace Wki.EventSourcing.Protocol
{
    // sent from EventStore -> Durable as first event during restore
    
    public class OfferSnapshot
    {
        public State State { get; private set; }

        public int LastEventId { get; private set; }

        public OfferSnapshot(State state, int lastEventId)
        {
            State = state;
            LastEventId = lastEventId;
        }
    }
}