namespace Wki.EventSourcing.Protocol.Subscription
{
    /// <summary>
    /// Command to EventStore to indicate events to subscribe to
    /// </summary>
    public class Subscribe
    {
        public InterestingEvents InterestingEvents { get; private set; }

        public Subscribe(InterestingEvents interestingEvents)
        {
            InterestingEvents = interestingEvents;
        }
    }
}
