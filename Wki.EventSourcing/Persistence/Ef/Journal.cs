using System;
using System.Collections.Generic;
using Wki.EventSourcing.Util;
using Newtonsoft.Json;
using System.Linq;

namespace Wki.EventSourcing.Persistence.Ef
{
    public class Journal : IJournalStore
    {
        public int LastEventId { get; private set; }

        private Dictionary<string, Type> EventTypeLookup;
        private EventStoreContext Context = new EventStoreContext();
        private JsonSerializerSettings JsonSettings = new JsonSerializerSettings
            {
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Formatting = Formatting.None,
            };

        public Journal()
        {
            var eventType = typeof(IEvent);

            EventTypeLookup = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(eventType.IsAssignableFrom))
                .ToDictionary(t => t.Name);
        }

        public void AppendEvent(string persistenceId, IEvent @event)
        {
            var eventRow = new EventRow
            {
                // Id = ???; // serial!
                PersistenceId = persistenceId,
                CreatedAt = SystemTime.Now,
                Type = @event.GetType().Name,
                Data = JsonConvert.SerializeObject(@event),
            };

            Context.EventRows.Add(eventRow);
            Context.SaveChanges();
        }

        public IEnumerable<EventRecord> LoadNextEvents(int fromPosExcluding, int nrEvents = 1000)
        {
            foreach (var row in Context.EventRows.Where(r => r.Id > fromPosExcluding).Take(nrEvents))
            {
                Type type = EventTypeLookup[row.Type];

                yield return new EventRecord(row.Id, row.CreatedAt, row.PersistenceId, JsonConvert.DeserializeObject(row.Data, type) as IEvent);
            }
        }

        public IEnumerable<EventRecord> LoadNextEvents(EventFilter filter, int nrEvents = 1000)
        {
            // TODO: nicht in SQL ausdrückbar.
            bool MatchesFilter(EventRow row)
            {
                if (row.Id <= filter.StartingAtIndexExcluding)
                    return false;

                if (filter.PersistenceId != null && filter.PersistenceId != row.PersistenceId)
                    return false;

                if (!filter.Events.Any())
                    return true;

                Type type = EventTypeLookup[row.Type];
                return filter.Events.Any(e => type.IsAssignableFrom(e));
            }

            foreach (var row in Context.EventRows.Where(MatchesFilter).Take(nrEvents))
            {
                Type type = null; // lookup based on row.Type

                yield return new EventRecord(row.Id, row.CreatedAt, row.PersistenceId, JsonConvert.DeserializeObject(row.Data, type) as IEvent);
            }
        }

        public bool HasSnapshot(string persistenceId) =>
            Context.SnapshotRows.Find(persistenceId) != null;

        public Snapshot<TState> LoadSnapshot<TState>(string persistenceId)
        {
            var snapshotRow = Context.SnapshotRows.Find(persistenceId);
            if (snapshotRow != null)
            {
                var state = JsonConvert.DeserializeObject<TState>(snapshotRow.Data);
                return new Snapshot<TState>(snapshotRow.CreatedAt, snapshotRow.LastEventId, state);
            }
            else
                return null;
        }

        public void SaveSnapshot<TState>(string persistenceId, TState state, int lastEventId)
        {
            var snapshotRow = new SnapshotRow
            {
                PersistenceId = persistenceId,
                CreatedAt = SystemTime.Now,
                LastEventId = lastEventId,
                Data = JsonConvert.SerializeObject(state),
            };
            Context.SnapshotRows.Add(snapshotRow);
            Context.SaveChanges();
        }
    }
}
