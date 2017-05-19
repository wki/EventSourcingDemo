namespace Wki.EventSourcing.Infrastructure
{
    /// <summary>
    /// Top level base class for a command for an aggregate root with an Id
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public class DispatchableCommand<TIndex>
    {
        public TIndex Id { get; private set; }

        public DispatchableCommand(TIndex id)
        {
            Id = id;
        }
    }
}
