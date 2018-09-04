using System.Collections.Generic;
using System.Linq;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// the in-memory representation of all events for fast lookup
    /// </summary>
    public class EventCache
    {
        private readonly List<EventRecord> Events = new List<EventRecord>();

        public int LastId { get => Events.Last().Id; }

        public void Add(EventRecord eventRecord) =>
            Events.Add(eventRecord);

        public void AddRange(IEnumerable<EventRecord> eventRecords) =>
            Events.AddRange(eventRecords);

        // TODO: eventuell sollten wir mit Indexen (z.B. PersistenceId, Type) arbeiten um effizienter zu filtern.
        public IEnumerable<EventRecord> NextEventsMatching(EventFilter eventFilter, int nrEvents = 1000) =>
            Events
                .Skip(FindIndex(eventFilter))
                .Where(eventFilter.Matches)
                .Take(nrEvents);

        // Binäre suche nach dem passenden Index. Id ist monoton steigend.
        private int FindIndex(EventFilter eventFilter)
        {
            if (eventFilter.StartAfterEventId <= 0)
                return 0;

            var min = 0;
            var max = Events.Count - 1;
            while (min < max)
            {
                var mid = min + (max - min) / 2;
                var midEventId = Events[mid].Id;
                if (midEventId < eventFilter.StartAfterEventId)
                    min = mid + 1;
                else if (midEventId > eventFilter.StartAfterEventId)
                    max = mid - 1;
                else
                    max = min;
            }

            return min > 0 ? min - 1 : min;
        }
    }
}