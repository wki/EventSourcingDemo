using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Answer from JournalReader containing a loaded event
    /// </summary>
    public class EventLoaded
    {
        public Event Event { get; private set; }

        public EventLoaded(Event @event)
        {
            Event = @event;
        }
    }
}
