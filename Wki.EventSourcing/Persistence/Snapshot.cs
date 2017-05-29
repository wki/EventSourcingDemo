using System;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// represents the state saved including its time and event position
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    public class Snapshot<TState>
    {
        public DateTime CreatedAt { get; private set; }
        public int LastEventPos { get; private set; }
        public TState State { get; private set; }

        public Snapshot(DateTime createdAt, int lastEventPos, TState state)
        {
            CreatedAt = createdAt;
            LastEventPos = lastEventPos;
            State = state;
        }
    }
}
