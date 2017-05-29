namespace Wki.EventSourcing.Protocol
{
    public class Persist
    {
        public IEvent Event { get; private set; }
        public string PersistenceId { get; private set; }

        public Persist(IEvent @event, string persistenceId)
        {
            Event = @event;
            PersistenceId = persistenceId;
        }
    }
}
