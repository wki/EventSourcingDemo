namespace Wki.EventSourcing
{
    /// <summary>
    /// an event sent out by an aggregate root
    /// </summary>
    public interface IEvent { }

    /// <summary>
    /// Base class for an event containing an Index
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public interface IEvent<TIndex> : IEvent
    {
        TIndex Id { get; }
    }
}
