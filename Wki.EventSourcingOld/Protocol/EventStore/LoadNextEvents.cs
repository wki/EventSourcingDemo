namespace Wki.EventSourcing.Protocol.EventStore
{
    /// <summary>
    /// Command to JournalReader to load all from journal
    /// </summary>
    public class LoadNextEvents
    {
        public int NrEvents { get; private set; }

        public LoadNextEvents(int nrEvents)
        {
            NrEvents = nrEvents;
        }
    }
}
