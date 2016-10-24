using System;
using Akka.Actor;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// extension method allowing to access our event store in a unified way
    /// </summary>
    public static class EventStoreExtension
    {
        private static IActorRef eventStore;

        public static IActorRef EventStore(this IUntypedActorContext context)
        {
            if (eventStore == null)
                eventStore =
                    context
                        .ActorSelection("/user/eventstore")
                        .ResolveOne(TimeSpan.FromSeconds(5))
                        .Result;

            return eventStore;
        }
    }
}
