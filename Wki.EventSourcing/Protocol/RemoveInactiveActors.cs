namespace Wki.EventSourcing.Protocol
{
    /// <summary>
    /// a timer tells an office to remove its inactive actors.
    /// </summary>
    public class RemoveInactiveActors 
    {
        public static RemoveInactiveActors Instance = new RemoveInactiveActors();

        private RemoveInactiveActors() {}
    }
}
