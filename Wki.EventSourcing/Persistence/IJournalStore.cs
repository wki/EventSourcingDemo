using System.Collections.Generic;
using Wki.EventSourcing.Protocol.Load;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// define the API needed for implementing a journal
    /// </summary>
    public interface IJournalStore
    {
        // last ID in DB (highest read or last written)
        int LastEventId { get; }

        // snapshot operations
        bool HasSnapshot(string persistenceId);
        void SaveSnapshot<TState>(string persistenceId, TState state, int lastEventId);
        Snapshot LoadSnapshot<TState>(string persistenceId);

        // journal operations
        void AppendEvent(string persistenceId, IEvent @event);
        IEnumerable<EventRecord> LoadNextEvents(int fromPosExcluding, int nrEvents = 1000);
        IEnumerable<EventRecord> LoadNextEvents(EventFilter filter, int nrEvents = 1000);
        // TODO: wie EOF entscheiden?
    }
}
