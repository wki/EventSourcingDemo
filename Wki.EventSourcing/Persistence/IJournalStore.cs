using System;
using System.Collections.Generic;
using Wki.EventSourcing.Protocol.Retrieval;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// define the API needed for implementing a synchronous journal
    /// </summary>
    public interface IJournalStore
    {
        /// <summary>
        /// last ID in DB (highest read or last written)
        /// </summary>
        /// <remarks>
        /// setting is needed to make tests work
        /// </remarks>
        int LastEventId { get; set; }

        /// <summary>
        /// Find out if we have a snapshot for a given persistenceId
        /// </summary>
        /// <param name="persistenceId"></param>
        /// <returns>true if present</returns>
        bool HasSnapshot(string persistenceId);

        /// <summary>
        /// Save a typed snapshot for a persistenceId
        /// </summary>
        /// <param name="persistenceId"></param>
        /// <param name="state"></param>
        /// <param name="lastEventId"></param>
        void SaveSnapshot(string persistenceId, object state, int lastEventId);

        /// <summary>
        /// Load a typed snapshot for a snapshot 
        /// </summary>
        /// <param name="persistenceId"></param>
        /// <param name="stateType"></param>
        /// <returns></returns>
        Snapshot LoadSnapshot(string persistenceId, Type stateType);

        
        /// <summary>
        /// Appends an event for a persistenceId
        /// </summary>
        /// <param name="persistenceId"></param>
        /// <param name="event"></param>
        void AppendEvent(string persistenceId, IEvent @event);

        /// <summary>
        /// Load a given count of events filtered
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="nrEvents"></param>
        /// <returns></returns>
        IEnumerable<EventRecord> LoadNextEvents(EventFilter filter, int nrEvents);
    }
}
