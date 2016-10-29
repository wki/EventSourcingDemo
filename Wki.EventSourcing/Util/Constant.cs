namespace Wki.EventSourcing.Util
{
    public static class Constant
    {
        /// <summary>
        /// Number of Events to restore as one Junk
        /// </summary>
        public const int NrRestoreEvents = 100;

        /// <summary>
        /// The low limit of a buffer triggering another request
        /// </summary>
        public const int BufferLowLimit = 10;
    }
}
