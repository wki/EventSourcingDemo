using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Answer from JournalWriter that event has been persisted
    /// </summary>
    public class EventPersisted
    {
        public Event Event { get; private set; }

        public EventPersisted(Event @event)
        {
            Event = @event;
        }
    }
}
