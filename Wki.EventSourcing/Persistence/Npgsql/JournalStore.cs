using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Wki.EventSourcing.Protocol.Load;
using static Wki.EventSourcing.Util.Serializer;

namespace Wki.EventSourcing.Persistence.Npgsql
{
    public class JournalStore : IJournalStore, IDisposable
    {
        public int LastEventId { get; private set; }

        #region queries
        private const string InsertEvent =
            @"insert into event 
             (persistence_id, type, data)
             values (@PersistenceId, @Type, @Data)
             returning id";

        private const string SelectEvents =
            @"select id, created_at, persistence_id, type, data
              from event
              where id > @FromPosExcluding
              limit @NrEvents";

        private const string ExistsSnapshot = "select exists(select * from snapshot where persistence_id = @PersistenceId)";

        private const string SelectSnapshot = "select created_at, last_event_id, data from snapshot where persistence_id = @PersistenceId";

        private const string InsertSnapshot =
            @"insert into snapshot
              (persistence_id, created_at, last_event_id, data)
              values (@PersistenceId, now(), @LastEventId, @Data)";

        private const string UpdateSnapshot =
            @"update snapshot
              set created_at = now(),
                  last_event_id = @LastEventId,
                  data = @Data
              where persistence_id = @PersistenceId";
        #endregion

        private NpgsqlConnection Connection;
        private Dictionary<string, Type> EventTypeLookup;


        public JournalStore()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["eventstore"].ConnectionString;

            Connection = new NpgsqlConnection(connectionString);
            Connection.Open();

            var eventType = typeof(IEvent);

            EventTypeLookup = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(eventType.IsAssignableFrom))
                .ToDictionary(t => t.Name);
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public void AppendEvent(string persistenceId, IEvent @event)
        {
            using (var cmd = new NpgsqlCommand(InsertEvent))
            {
                cmd.Parameters.AddWithValue("PersistenceId", NpgsqlTypes.NpgsqlDbType.Text, persistenceId);
                cmd.Parameters.AddWithValue("Type", NpgsqlTypes.NpgsqlDbType.Text, @event.GetType().Name);
                cmd.Parameters.AddWithValue("Data", NpgsqlTypes.NpgsqlDbType.Text, Serialize(@event));

                cmd.Prepare();

                LastEventId = (int)cmd.ExecuteScalar();
            }
        }

        public IEnumerable<EventRecord> LoadNextEvents(int fromPosExcluding, int nrEvents = 1000)
        {
            using (var cmd = new NpgsqlCommand(SelectEvents))
            {
                cmd.Parameters.AddWithValue("FromPosExcluding", NpgsqlTypes.NpgsqlDbType.Integer, fromPosExcluding);
                cmd.Parameters.AddWithValue("NrEvents", NpgsqlTypes.NpgsqlDbType.Integer, nrEvents);

                cmd.Prepare();

                using (var reader = cmd.ExecuteReader())
                {
                    yield return new EventRecord(
                        reader.GetInt32(0),
                        reader.GetDateTime(1),
                        reader.GetString(2),
                        (IEvent)Deserialize(reader.GetString(4), EventTypeLookup[reader.GetString(3)])
                    );
                }
            }
        }

        public IEnumerable<EventRecord> LoadNextEvents(EventFilter filter, int nrEvents = 1000)
        {
            throw new NotImplementedException();
        }

        public bool HasSnapshot(string persistenceId)
        {
            using (var cmd = new NpgsqlCommand(ExistsSnapshot))
            {
                cmd.Parameters.AddWithValue("PersistenceId", NpgsqlTypes.NpgsqlDbType.Text, persistenceId);
                cmd.Prepare();
                return (bool)cmd.ExecuteScalar();
            }
        }

        public Snapshot LoadSnapshot(string persistenceId, Type stateType)
        {
            using (var cmd = new NpgsqlCommand(SelectSnapshot))
            {
                cmd.Parameters.AddWithValue("PersistenceId", NpgsqlTypes.NpgsqlDbType.Text, persistenceId);
                cmd.Prepare();
                using (var reader = cmd.ExecuteReader())
                {
                    return new Snapshot(
                        Deserialize(reader.GetString(2), stateType),
                        reader.GetInt32(1)
                    );
                }
            }
        }

        public void SaveSnapshot<TState>(string persistenceId, TState state, int lastEventId)
        {
            var sql = HasSnapshot(persistenceId) ? UpdateSnapshot : InsertSnapshot;
            using (var cmd = new NpgsqlCommand(sql))
            {
                cmd.Parameters.AddWithValue("PersistenceId", NpgsqlTypes.NpgsqlDbType.Text, persistenceId);
                cmd.Parameters.AddWithValue("LastEventId", NpgsqlTypes.NpgsqlDbType.Integer, lastEventId);
                cmd.Parameters.AddWithValue("Data", NpgsqlTypes.NpgsqlDbType.Text, Serialize(state));

                cmd.Prepare();

                cmd.ExecuteNonQuery();
            }
        }
    }
}
