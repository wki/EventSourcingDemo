namespace Wki.EventSourcing.Protocol.Subscription
{
    /// <summary>
    /// Start a subscription at eventStore
    /// </summary>
    public class Subscribe
    {
        public EventFilter EventFilter { get; private set; }

        public Subscribe(EventFilter eventFilter)
        {
            EventFilter = eventFilter;
        }
    }

    /// <summary>
    /// Cancel a subscription
    /// </summary>
    public class Unsubscribe
    {
        public static Unsubscribe Instance = new Unsubscribe();

        private Unsubscribe() {}
    }
}
