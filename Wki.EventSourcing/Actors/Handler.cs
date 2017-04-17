using System;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Abstraction of a delegate for a certain Message Handler
    /// </summary>
    /// <example>
    /// List<Handler> events;
    ///
    /// protected void Recover<E>(Action<E> eventHandler) =>
    ///     events.Add(typeof(E), new Handler(typeof(E), e => eventHandler((E)e)));
    /// </example>
    public class Handler
    {
        public Type Type { get; private set; }
        public Action<object> Handle { get; private set; }

        public Handler(Type type, Action<object> handle)
        {
            Type   = type;
            Handle = handle;
        }
    }
}
