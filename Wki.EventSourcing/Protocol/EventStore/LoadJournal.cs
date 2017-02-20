namespace Wki.EventSourcing.Protocol.EventStore
{
    /// <summary>
    /// Command to JournalReader to load all from journal
    /// </summary>
    public class LoadJournal
    {
        public int NrEvents { get; private set; }

        public LoadJournal(int nrEvents)
        {
            NrEvents = nrEvents;
        }
    }
}
