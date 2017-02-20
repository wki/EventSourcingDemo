namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// Possible states for a durable actor during its life cycle
    /// </summary>
    public enum DurableActorStatus
    {
        Initializing,
        Restoring,
        Operating,
        Dead,
        Stopping
    }
}
