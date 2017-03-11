namespace Wki.EventSourcing.Protocol.Statistics
{
    public enum OfficeActorChildStatus
    {
        Operating,          // regular operation -- nothing to do
        DiedDuringRestore   // could not restore -- do not reuse

        // not needed.
        // Dead,               // did not respond with still alive for too long -- restart

    }
}
