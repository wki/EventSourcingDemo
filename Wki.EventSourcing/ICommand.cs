namespace Wki.EventSourcing
{
    /// <summary>
    /// a command for an aggregate root
    /// </summary>
    public interface ICommand { }

    /// <summary>
    /// a command for an aggregate root with an Id
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public interface ICommand<TIndex>: ICommand
    {
        TIndex Id { get; }
    }
}
