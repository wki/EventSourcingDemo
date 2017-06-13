﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Wki.EventSourcing
{
    public class EventFilter
    {
        // TODO: performance benchmark.
        // evtl. List<Type> durch HashSet<Type> mit allen abgeleiteten Typen ersetzen...
        public string PersistenceId { get; set; } = null;
        public List<Type> Events { get; set; } = new List<Type>();
        public int StartingAtIndexExcluding { get; set; } = -1;

        #region Builder DSL
        public EventFilter ForPersistenceId(string persistenceId)
        {
            PersistenceId = persistenceId;
            return this;
        }

        public EventFilter AnyPersistenceId()
        {
            PersistenceId = null;
            return this;
        }

        public EventFilter When<TEvent>()
            where TEvent: IEvent
        {
            Events.Add(typeof(TEvent));
            return this;
        }

        public EventFilter AnyEvent()
        {
            Events.Clear();
            return this;
        }

        public EventFilter StartingAtExcluding(int index)
        {
            StartingAtIndexExcluding = index;
            return this;
        }

        public EventFilter StartFromBeginning() => 
            StartingAtExcluding(-1);

        public EventFilter OnlyFutureEvents() => 
            StartingAtExcluding(Int32.MaxValue);
        #endregion

        #region matching
        public bool Matches(string persistenceId, Type type, int id = Int32.MaxValue) =>
            (PersistenceId == null || PersistenceId == persistenceId)
            && id > StartingAtIndexExcluding
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
    public static class WantEvents
    {
        public static EventFilter ForPersistenceId(string persistenceId) =>
            new EventFilter().ForPersistenceId(persistenceId);

        public static EventFilter AnyPersistenceId() =>
            new EventFilter().AnyPersistenceId();

        public static EventFilter When<TEvent>() where TEvent : IEvent =>
            new EventFilter().When<TEvent>();

        public static EventFilter AnyEvent() =>
            new EventFilter().AnyEvent();

        public static EventFilter StartingAtExcluding(int index) =>
            new EventFilter().StartingAtExcluding(index);

        public static EventFilter StartFromBeginning() =>
            new EventFilter().StartFromBeginning();

        public static EventFilter OnlyFutureEvents() =>
            new EventFilter().OnlyFutureEvents();
    }
}