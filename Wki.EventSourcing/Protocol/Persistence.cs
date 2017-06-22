using System;

namespace Wki.EventSourcing.Protocol.Persistence
{
    // Persist Protocol: Persist Snapshots and events

    /// <summary>
    /// Request to persist an event for a given PersistenceId
    /// </summary>
    public class PersistEvent
    {
        public IEvent Event { get; private set; }
        public string PersistenceId { get; private set; }

        public PersistEvent(string persistenceId, IEvent @event)
        {
            PersistenceId = persistenceId;
            Event = @event;
        }
    }

    /// <summary>
    /// Negative result of a persist: failure
    /// </summary>
    public class PersistEventFailed
    {
        public IEvent Event { get; private set; }
        public string PersistenceId { get; private set; }
        public string Message { get; private set; }

        public PersistEventFailed(string persistenceId, IEvent @event, string message)
        {
            PersistenceId = persistenceId;
            Event = @event;
            Message = message;
        }
    }

    /// <summary>
    /// Positive result of a persist for applying and subscriptions
    /// </summary>
    public class EventPersisted
    {
        public EventRecord EventRecord { get; private set; }

        public EventPersisted(EventRecord eventRecord)
        {
            EventRecord = eventRecord;
        }
    }

    /// <summary>
    /// Request to persist a snapshot for a PersistenceId
    /// </summary>
    public class PersistSnapshot
    {
        public string PersistenceId { get; private set; }

        public object State { get; private set; }

        public int LastEventId { get; private set; }

        public PersistSnapshot(string persistenceId, object state, int lastEventId)
        {
            PersistenceId = persistenceId;
            State = state;
            LastEventId = lastEventId;
        }
    }

    ///// <summary>
    ///// Exception indicating failure during Persisting
    ///// </summary>
    //public class PersistTimeoutException : Exception
    //{
    //    public PersistTimeoutException(string message) : base(message) { }
    //}
}
