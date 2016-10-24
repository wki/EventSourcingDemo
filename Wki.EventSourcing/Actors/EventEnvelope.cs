using System;
using Wki.EventSourcing.Messages;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Hold all housekeeping information for an event together
    /// </summary>
    public class EventEnvelope
    {
        public string PersistenceId { get; set; }
        public Event Event { get; set; }

        public EventEnvelope(string persistenceId, Event @event)
        {
            PersistenceId = persistenceId;
            Event = @event;
        }
    }
}
