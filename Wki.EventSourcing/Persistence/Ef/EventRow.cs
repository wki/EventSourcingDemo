using System;

namespace Wki.EventSourcing.Persistence.Ef
{
    /// <summary>
    /// Representation of a serialized eventRecord in our Database
    /// </summary>
    public class EventRow
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string PersistenceId { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
    }
}
