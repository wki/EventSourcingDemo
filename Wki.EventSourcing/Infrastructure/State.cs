namespace Wki.EventSourcing.Infrastructure
{
    /// <summary>
    /// Base class for an immutable state getting updated by events
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    abstract public class State<TState>
    {
        /// <summary>
        /// updates a state with an event
        /// </summary>
        /// <param name="event"></param>
        /// <returns>a new state instance</returns>
        public abstract State<TState> Update(DomainEvent @event);
    }
}
