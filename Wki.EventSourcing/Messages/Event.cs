using System;

namespace Wki.EventSourcing.Messages
{
    /// <summary>
    /// Abstract base class for an event
    /// </summary>
    public abstract class Event {}

    public class Event<TIndex> : Event
    {
        /// <summary>
        /// The time the event occured.
        /// </summary>
        /// <value>The occured on.</value>
        public DateTime OccuredOn { get; private set; }

        /// <summary>
        /// Identifier for the aggregate root involved. May be null
        /// </summary>
        /// <value>The persistence identifier.</value>
        public TIndex Id { get; protected set; }

        public Event() : this(DateTime.Now) { }
        public Event(TIndex id) : this(DateTime.Now, id) { }
        public Event(DateTime occuredOn) : this(occuredOn, default(TIndex)) { }
        public Event(DateTime occuredOn, TIndex id)
        {
            OccuredOn = occuredOn;
            Id = id;
        }
    }
}
