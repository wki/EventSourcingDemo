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
        private static Unsubscribe _instance;

        public static Unsubscribe Instance =>
            _instance ?? (_instance = new Unsubscribe());

        private Unsubscribe() {}
    }
}
