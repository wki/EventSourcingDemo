using System.Collections.Generic;
using System.Linq;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// the in-memory representation of all events for fast lookup
    /// </summary>
    public class EventCache
    {
        private List<EventRecord> Events = new List<EventRecord>();
        public int LastId { get => Events.Last().Id; }

        public void Append(EventRecord eventRecord)
        {
            Events.Add(eventRecord);
        }

        // TODO: eventuell sollten wir mit Indexen (z.B. PersistenceId, Type) arbeiten um effizienter zu filtern.
        public IEnumerable<EventRecord> NextEventsMatching(EventFilter eventFilter, int nrEvents = 1000) =>
            Events
                .Where(eventFilter.Matches)
                .Take(nrEvents);
    }
}