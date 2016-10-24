using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Abstract base class for a list of interesting events
    /// </summary>
    public abstract class InterestingEvents
    {
        public abstract bool Matches(Event @event);
    }

    /// <summary>
    /// Generic implementation of interesting events for a given index type
    /// </summary>
    public class InterestingEvents<TIndex> : InterestingEvents
    {
        public TIndex Id { get; private set; }
        public ImmutableHashSet<Type> Events { get; private set; }

        public InterestingEvents() : this(default(TIndex), null) { }

        public InterestingEvents(TIndex id, IEnumerable<Type> events)
        {
            Id = id;
            Events = (events ?? new Type[] { }).ToImmutableHashSet();
        }

        public override bool Matches(Event @event)
        {
            var typedEvent = @event as Event<TIndex>;

            // when cast fails, event cannot be right (different TIndex)
            if (typedEvent == null)
                return false;

            // filter the interesting events
            if (!Events.Contains(@event.GetType()))
                return false;

            // if we are interested in an id, it must be the same
            if (Id != null && !typedEvent.Id.Equals(Id))
                return false;

            return true;
        }
    }
}
