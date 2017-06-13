namespace Wki.EventSourcing.Protocol.LiveCycle
{
    /// <summary>
    /// A Durable actor indicates to its office that it is still alive
    /// </summary>
    public class StillAlive
    {
        public static StillAlive Instance = new StillAlive();

        private StillAlive() {}
    }

    /// <summary>
    /// a durable actor indicates to its office that it wants to get passivated
    /// </summary>
    public class Passivate
    {
        public static Passivate Instance = new Passivate();

        private Passivate() {}
    }

    /// <summary>
    /// a timer tells an office to remove its inactive actors.
    /// </summary>
    public class RemoveInactiveActors
    {
        public static RemoveInactiveActors Instance = new RemoveInactiveActors();

        private RemoveInactiveActors() {}
    }

}
