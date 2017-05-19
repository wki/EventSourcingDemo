using Wki.EventSourcing.Infrastructure;

namespace Wki.EventSourcing.Protocol
{
    public class Persist
    {
        public DomainEvent Event { get; private set; }
        public string PersistenceId { get; private set; }

        public Persist(DomainEvent @event, string persistenceId)
        {
            Event = @event;
            PersistenceId = persistenceId;
        }
    }
}
