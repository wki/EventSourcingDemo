using System;

namespace Wki.EventSourcing.Infrastructure
{
    /// <summary>
    /// Captures an event and its metadata for transporting to subscribers
    /// </summary>
    public class EventRecord
    {
        public int Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string PersistenceId { get; private set; }
        public DomainEvent Event { get; private set; }

        public EventRecord(int id, DateTime createdAt, string persistenceId, DomainEvent @event)
        {
            Id = id;
            CreatedAt = createdAt;
            PersistenceId = persistenceId;
            Event = @event;
        }
    }
}
