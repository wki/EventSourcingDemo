namespace Wki.EventSourcing
{
    /// <summary>
    /// Interface for an immutable state getting updated by events
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <example>
    /// public class Xxx : IState<Xxx>
    /// {
    ///     #region commands
    ///     public class Command : DispatchableCommand<int>
    ///     {
    ///         public Command(int id) : base(id) { }
    ///     }
    /// 
    ///     public class DoSomething : Command
    ///     {
    ///         public DoSomething(int id) : base(id) { }
    ///     }
    ///     #endregion
    /// 
    ///     #region events
    ///     public class Event : DomainEvent<int>
    ///     {
    ///         public Event(int id) : base(id) { }
    ///     }
    /// 
    ///     public class SomethingDone : Event
    ///     {
    ///         public SomethingDone(int id) : base(id) { }
    ///     }
    ///     #endregion
    /// 
    ///     public Xxx()
    ///     {
    ///     }
    /// 
    ///     public IState<Xxx> Apply(DomainEvent @event)
    ///     {
    ///         switch(@event)
    ///         {
    ///             case SomethingDone s:
    ///                 return new Xxx(/* changed arguments */);
    ///             default:
    ///                 throw new ArgumentException($"cannot handle event of type {@event.GetType().FullName}");
    ///         }
    ///     }
    /// }
    /// </example>
    public interface IState<TState>
    {
        /// <summary>
        /// applies an event to the current state returning a new state
        /// </summary>
        /// <param name="event"></param>
        /// <returns>a new state instance</returns>
        IState<TState> Apply(IEvent @event);
    }
}
