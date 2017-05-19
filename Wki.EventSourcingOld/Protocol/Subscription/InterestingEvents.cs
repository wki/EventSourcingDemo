using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace Wki.EventSourcing.Protocol.Subscription
{
    /// <summary>
    /// a list of interesting events without an Id
    /// </summary>
    public class InterestingEvents
    {
        public ImmutableHashSet<Type> Events { get; private set; }

        public InterestingEvents() : this(null) {}
        public InterestingEvents(IEnumerable<Type> events)
        {
            Events = (events ?? new Type[] { })
                .Where(e => e != null)
                .ToImmutableHashSet();
        }

        public virtual bool Matches(Event @event) =>
            Events.Contains(@event.GetType());

        protected string ListEvents() =>
            String.Join(", ", Events.Select(e => e.Name).OrderBy(e => e));
        
        public override string ToString() =>
            String.Format("[InterestingEvents: {0}]", ListEvents());
    }

    /// <summary>
    /// Generic implementation of interesting events for a given index type
    /// </summary>
    public class InterestingEvents<TIndex> : InterestingEvents
    {
        public TIndex Id { get; private set; }

        public InterestingEvents() : this(default(TIndex), null) { }

        public InterestingEvents(TIndex id, IEnumerable<Type> events) : base(events)
        {
            Id = id;
        }

        public override bool Matches(Event @event)
        {
            var typedEvent = @event as Event<TIndex>;

            // when cast fails, event cannot be right (different TIndex)
            return typedEvent != null
                && base.Matches(@event)
                && typedEvent.Id.Equals(Id);
        }

        public override string ToString() =>
            String.Format("[InterestingEvents Id={0}: {1}]", Id.ToString(), ListEvents());

    }
}
