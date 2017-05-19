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

        // idle time in seconds after which a durable actor with ID is asking for passivation
        public static readonly TimeSpan MaxActorIdleTimeSpan = TimeSpan.FromSeconds(120); // live: 300

        // periodic trigger of an actor for reporting "still alive" 
        public static readonly TimeSpan ActorStillAliveInterval = TimeSpan.FromSeconds(30);

        // periodic cleanup interval for clerks not reporting "still alive"
        public static readonly TimeSpan DeadActorRemoveTimeSpan = TimeSpan.FromSeconds(120);

        // default nr of events to restore in one block
        public const int DefaultNrEvents = 1000;


    }
}
