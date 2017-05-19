using System;

namespace Wki.EventSourcing.Serialisation
{
    /// <summary>
    /// encapsulates an Event's json string with additional information
    /// </summary>
    public class Envelope
    {
        /// <summary>
        /// The serialized Json of the event without type information inside/> 
        /// </summary>
        /// <value>Json string</value>
        public string Json { get; private set; }

        /// <summary>
        /// The abbreviated data type. Must be unique inside the whole domain
        /// </summary>
        /// <value>short name</value>
        public string DataType { get; private set; }

        /// <summary>
        /// optional Id of the event if set. Allows filtering on the storage
        /// </summary>
        /// <value>The identifier.</value>
        public string Id { get; private set; }

        /// <summary>
        /// Replication of the timestamp the event occured. Allows filtering on the storage
        /// </summary>
        /// <value>The occured on.</value>
        public DateTime OccuredOn { get; private set; }

        public Envelope(string json, Type type, string id, DateTime occuredOn)
        {
            Json = json;
            DataType = type.Name;
            Id = id;
            OccuredOn = occuredOn;
        }
    }
}
