using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Command dispatchable to an actor by specifying an Id
    /// </summary>
    /// <description>
    /// In order to be able to dispatch a command an actor ID
    /// must be inside the command.
    /// </description>
    public class DispatchableCommand<TIndex>
    {
        public TIndex Id { get; private set; }

        public DispatchableCommand(TIndex id)
        {
            Id = id;
        }
    }
}
