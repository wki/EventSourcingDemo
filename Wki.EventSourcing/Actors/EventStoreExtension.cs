using System;
using Akka.Actor;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// extension method allowing to access our event store in a unified way
    /// </summary>
    public static class EventStoreExtension
    {
        // allow setting the value for a test
        public static IActorRef eventStore;

        /// <summary>
        /// Get the event store creating it if not present
        /// </summary>
        /// <returns>The store.</returns>
        /// <param name="context">Context.</param>
        public static IActorRef EventStore(this IUntypedActorContext context)
        {
            if (eventStore == null)
                eventStore = context.System.ActorOf(Props.Create<EventStore>(), "eventstore");

            return eventStore;
        }
    }
}
