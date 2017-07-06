using System;
using System.Collections.Generic;
using System.Linq;

namespace Wki.EventSourcing
{
    public class EventFilter
    {
        // a secret event type that never occurs anywhere
        private class NotMatchedEvent: IEvent {}

        // TODO: performance benchmark.
        // evtl. List<Type> durch HashSet<Type> mit allen abgeleiteten Typen ersetzen...
        public string PersistenceId { get; set; } = null;
        public List<Type> Events { get; set; } = new List<Type>();
        public int StartAfterEventId { get; set; } = -1;

        public bool FiltersPersistenceId =>
            PersistenceId != null;

        public bool FiltersEvents =>
            Events.Any();

        #region Builder DSL
        public EventFilter For(string persistenceId)
        {
            PersistenceId = persistenceId;
            return this;
        }

        public EventFilter All() =>
            For(null);

        public EventFilter Allow(Type type)
        {
            Events.Add(type);
            return this;
        }

        public EventFilter Allow<TEvent>() where TEvent : IEvent =>
            Allow(typeof(TEvent));

        public EventFilter AnyEvent()
        {
            Events.Clear();
            return this;
        }

        public EventFilter NoEvent() =>
            AnyEvent().Allow<NotMatchedEvent>();

        public EventFilter After(int index)
        {
            StartAfterEventId = index;
            return this;
        }

        public EventFilter FromBeginning() => 
            After(-1);

        public EventFilter OnlyFuture() => 
            After(Int32.MaxValue);
        #endregion

        #region matching
        public bool Matches(string persistenceId, Type type, int id = Int32.MaxValue) =>
            (PersistenceId == null || PersistenceId == persistenceId)
            && id > StartAfterEventId
            && Events.Count == 0 || Events.Any(e => type.IsAssignableFrom(e));

        public bool Matches(string persistenceId, IEvent @event, int id = Int32.MaxValue) =>
            Matches(persistenceId, @event.GetType(), id);

        public bool Matches(EventRecord eventRecord) =>
            Matches(eventRecord.PersistenceId, eventRecord.Event.GetType(), eventRecord.Id);

        #endregion
    }

    /// <summary>
    /// DSL for setting up an EventFilter
    /// </summary>
    /// <example>
    /// WantEvents
    ///         // persitenceId - default: no filter
    ///     .All()
    ///     .For("product-42")
    ///     
    ///         // Events - default: no filter
    ///     .NoEvent()
    ///     .AnyEvent()
    ///     .Allow(typeof(E1))
    ///     .Allow<E1>()
    ///     
    ///         // index - default: from beginning
    ///     .FromBeginning()
    ///     .After(4711)
    ///     .OnlyFuture()
    /// 
    /// </example>
    public static class WantEvents
    {
        public static EventFilter For(string persistenceId) =>
            new EventFilter().For(persistenceId);

        public static EventFilter All() =>
            new EventFilter().All();

        public static EventFilter Allow(Type type) =>
            new EventFilter().Allow(type);

        public static EventFilter Allow<TEvent>() where TEvent : IEvent =>
            new EventFilter().Allow<TEvent>();

        public static EventFilter AnyEvent() =>
            new EventFilter().AnyEvent();

        public static EventFilter NoEvent() =>
            new EventFilter().NoEvent();

        public static EventFilter After(int index) =>
            new EventFilter().After(index);

        public static EventFilter FromBeginning() =>
            new EventFilter().FromBeginning();

        public static EventFilter OnlyFuture() =>
            new EventFilter().OnlyFuture();
    }
}
