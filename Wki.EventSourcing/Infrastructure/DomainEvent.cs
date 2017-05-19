namespace Wki.EventSourcing.Infrastructure
{
    /// <summary>
    /// Base class for a Domain Event
    /// </summary>
    public class DomainEvent { }

    /// <summary>
    /// Base class for a Domain Event containing an Index
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public class DomainEvent<TIndex> : DomainEvent
    {
        public TIndex Id { get; private set; }

        public DomainEvent(TIndex id)
        {
            Id = id;
        }
    }
}
