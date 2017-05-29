namespace Wki.EventSourcing.Protocol
{
    public class Subscribe
    {
        public EventFilter EventFilter { get; private set; }

        public Subscribe(EventFilter eventFilter)
        {
            EventFilter = eventFilter;
        }
    }
}
