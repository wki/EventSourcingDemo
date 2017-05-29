using System;

namespace Wki.EventSourcing.Persistence.Ef
{
    /// <summary>
    /// Representation of a serialized snapshot in our DB
    /// </summary>
    public class SnapshotRow
    {
        public string PersistenceId { get; set; }
        public DateTime CreatedAt { get; set; }
        public int LastEventId { get; set; }
        public string Data { get; set; }
    }
}
