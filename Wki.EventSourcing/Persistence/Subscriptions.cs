using Akka.Actor;
using System.Collections;
using System.Collections.Generic;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// Handles all subscriptions made by various actors
    /// </summary>
    public class Subscriptions
    {
        private Dictionary<IActorRef, EventFilter> Subscribers = new Dictionary<IActorRef, EventFilter>();

        /// <summary>
        /// An actor subscribes to events
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="wantEvents"></param>
        public void Subscribe(IActorRef subscriber, EventFilter wantEvents) =>
            Subscribers[subscriber] = wantEvents;

        /// <summary>
        /// An actor unsubscribes from any events
        /// </summary>
        /// <param name="subscriber"></param>
        public void Unsubscribe(IActorRef subscriber)
        {
            if (Subscribers.ContainsKey(subscriber))
                Subscribers.Remove(subscriber);
        }

        /// <summary>
        /// Return all actors subscribed for a given event
        /// </summary>
        /// <param name="eventRecord"></param>
        /// <returns></returns>
        public IEnumerable<IActorRef> ActorsSubscribedFor(EventRecord eventRecord)
        {
            foreach (var subscriber in Subscribers.Keys)
                if (Subscribers[subscriber].Matches(eventRecord))
                    yield return subscriber;
        }

        public EventFilter EventsWantedFor(IActorRef actor)
        {
            if (!Subscribers.ContainsKey(actor))
                return WantEvents.NoEvent();
            else
                return Subscribers[actor];
        }
    }
}
