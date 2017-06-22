namespace Wki.EventSourcing.Protocol.LifeCycle
{
    /// <summary>
    /// A Durable actor indicates to its office that it is still alive
    /// </summary>
    public class StillAlive
    {
        private static StillAlive _instance;

        public static StillAlive Instance =>
            _instance ?? (_instance = new StillAlive());

        private StillAlive() {}
    }

    /// <summary>
    /// a durable actor indicates to its office that it wants to get passivated
    /// </summary>
    public class Passivate
    {
        private static Passivate _instance;

        public static Passivate Instance =>
            _instance ?? (_instance = new Passivate());

        private Passivate() {}
    }

    /// <summary>
    /// a timer tells an office to remove its inactive actors.
    /// </summary>
    public class RemoveInactiveActors
    {
        private static RemoveInactiveActors _instance;

        public static RemoveInactiveActors Instance =>
            _instance ?? (_instance = new RemoveInactiveActors());

        private RemoveInactiveActors() {}
    }

}
