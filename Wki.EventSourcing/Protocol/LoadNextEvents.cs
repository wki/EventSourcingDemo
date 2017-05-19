namespace Wki.EventSourcing.Protocol
{
    /// <summary>
    /// Request Events from EventStore or Journal starting at a given position
    /// </summary>
    public class LoadNextEvents
    {
        public int FromPosExcluding { get; private set; }

        private LoadNextEvents(int fromPosExcluding)
        {
            FromPosExcluding = fromPosExcluding;
        }

        public static LoadNextEvents StartingAt(int fromPosExcluding) =>
            new LoadNextEvents(fromPosExcluding);
    }
}
