namespace Wki.EventSourcing.Protocol
{
    /// <summary>
    /// Request Events from EventStore or Journal starting at a given position
    /// </summary>
    public class LoadNextEvents
    {
        public int FromPosExcluding { get; private set; }
        public int NrEvents { get; private set; }

        private LoadNextEvents(int fromPosExcluding, int nrEvents = 1000)
        {
            FromPosExcluding = fromPosExcluding;
            NrEvents = nrEvents;
        }

        public static LoadNextEvents FromBeginning => 
            StartingAt(-1);

        public static LoadNextEvents StartingAt(int fromPosExcluding) =>
            new LoadNextEvents(fromPosExcluding);
    }
}
