using System;

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

        // idle time in seconds after which an actor is passivating itself
        public static readonly TimeSpan MaxActorIdleTimeSpan = TimeSpan.FromMinutes(2); // live: 5

        // periodic trigger of an actor for reporting "stillalive" 
        public static readonly TimeSpan ActorStillAliveInterval = TimeSpan.FromSeconds(30);

        // periodic poll for inactive actors inside an office
        public static readonly TimeSpan IdleActorPollTimeSpan = TimeSpan.FromSeconds(60);

        // default nr of events to restore in one block
        public const int DefaultNrEvents = 1000;


    }
}
