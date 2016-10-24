using System;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Abstraction of a delegate for a certain Message Handler
    /// </summary>
    public class Handler
    {
        public Type Type { get; private set; }
        public Action<object> Action { get; private set; }

        public Handler(Type type, Action<object> action)
        {
            Type = type;
            Action = action;
        }
    }
}
