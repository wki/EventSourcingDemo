namespace Wki.EventSourcing.Protocol.EventStore
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
