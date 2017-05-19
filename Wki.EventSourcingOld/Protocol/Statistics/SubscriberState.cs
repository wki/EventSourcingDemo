using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol.Subscription;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Protocol.Statistics
{
    /// <summary>
    /// Represent the state of a subscriber registered at eventstore
    /// </summary>
    public class SubscriberState
    {
        /// <summary>
        /// The subscribing actor
        /// </summary>
        public IActorRef Actor { get; set; }

        /// <summary>
        /// State: restoring? operating otherwise
        /// </summary>
        public bool Restoring { get; set; }

        /// <summary>
        /// When did we send an Event to this subscriber the last time
        /// </summary>
        public DateTime LastSent { get; set; }

        /// <summary>
        /// only during restore: What is the next message index to receive
        /// </summary>
        public int NextMessageIndex { get; set; }

        /// <summary>
        /// Summary of events meant for the subscriber
        /// </summary>
        public InterestingEvents InterestingEvents { get; set; }

        public SubscriberState(IActorRef actor, InterestingEvents interestingEvents)
        {
            Actor = actor;
            Restoring = true;
            LastSent = SystemTime.Now;
            NextMessageIndex = 0;
            InterestingEvents = interestingEvents;
        }

        /// <summary>
        /// Check a given event if it is interesting for a subscriber
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        public bool IsInterestedIn(Event @event) => InterestingEvents.Matches(@event);

        /// <summary>
        /// update statistics after subscriber received n event
        /// </summary>
        internal void ReceivedEvent()
        {
            LastSent = SystemTime.Now;
        }

        /// <summary>
        /// stringify
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var restoreFlag = Restoring ? ">" : "=";
            var age = (SystemTime.Now - LastSent).TotalSeconds;

            return string.Format($"{restoreFlag}{Actor.Path}-{age:N0}");
        }
    }
}
