using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// An actor with persisted state without an Id
    /// </summary>
    /// <example>
    /// public class MyActor : DurableActor
    /// {
    ///     public MyActor(IActorRef eventStore) : base(eventStore)
    ///     {
    ///         // specify events (for restoring or changing state)
    ///         Recover<SomethingHappened>(...);
    /// 
    ///         // specify other messages (commands, queries, ...)
    ///         Receive<LetSomethingHappen>(...);
    ///     }
    /// }
    /// 
    /// </example>
    public abstract class DurableActor : UntypedActor, IWithUnboundedStash
    {
        /// <summary>
        /// Stash for unhandled messages. Will be processed after restore
        /// </summary>
        /// <value>The stash.</value>
        public IStash Stash { get; set; }

        /// <summary>
        /// Indicate the running restore process
        /// </summary>
        /// <value><c>true</c> if currently restoring; otherwise, <c>false</c>.</value>
        public bool IsRestoring { get; set; }

        // keep all events and commands we may respond to
        protected List<Handler> commands;
        protected List<Handler> events;

        // we must know our event store. Typically the office will tell us
        private readonly IActorRef eventStore;

        // during restore count events for aquiring next junk before completion
        // we keep track of the nr of events we still await to receive
        private int eventsToReceive;

        protected DurableActor(IActorRef eventStore)
        {
            if (eventStore == null)
                throw new ArgumentNullException(nameof(eventStore));

            this.eventStore = eventStore;

            commands = new List<Handler>();
            events = new List<Handler>();

            IsRestoring = false;
        }

        protected override void PreStart()
        {
            base.PreStart();
            StartRestoring();
        }

        protected override void PostRestart(Exception reason)
        {
            base.PostRestart(reason);
            StartRestoring();
        }

        protected virtual InterestingEvents GenerateInterestingEvents()
        {
            return new InterestingEvents(events.Select(e => e.Type));
        }

        #region Event and Command Handling
        /// <summary>
        /// Declare an event to handle
        /// </summary>
        /// <param name="eventHandler">Event handler.</param>
        /// <typeparam name="E">The 1st type parameter.</typeparam>
        protected void Recover<E>(Action<E> eventHandler) =>
            events.Add(new Handler(typeof(E), e => eventHandler((E)e)));

        /// <summary>
        /// Receive an event (alias to command)
        /// </summary>
        /// <param name="eventHandler">Event handler.</param>
        /// <typeparam name="E">Type of the event to receive</typeparam>
        protected void Receive<E>(Action<E> eventHandler) =>
            commands.Add(new Handler(typeof(E), c => eventHandler((E)c)));

        /// <summary>
        /// Persist (and subsequently handle) an event
        /// </summary>
        /// <param name="event">Message.</param>
        protected void Persist(Event @event)
        {
            Context.System.Log.Debug("Actor {0}: Persist {1}", Self.Path.Name, @event.GetType().Name);

            eventStore.Tell(new PersistEvent(@event));
        }
        #endregion

        #region regular operation
        /// <summary>
        /// Regular Operation: receive Commands and Events
        /// </summary>
        /// <param name="message">Message.</param>
        protected override void OnReceive(object message)
        {
            var handler =
                events.Find(e => e.Type == message.GetType())
                      ?? commands.Find(c => c.Type == message.GetType());

            if (handler != null)
                handler.Action(message);
            else
                Unhandled(message);
        }
        #endregion

        #region restore state
        /// <summary>
        /// Behaviour during restore: Receive events but stash other messages
        /// </summary>
        /// <param name="message">Message.</param>
        private void Restoring(object message)
        {
            if (message is End)
            {
                Context.System.Log.Debug("Actor {0}: CompletedRestore", Self.Path.Name);

                IsRestoring = false;
                UnbecomeStacked();
                Stash.UnstashAll();
            }
            else
            {
                var eventHandler = events.Find(e => e.Type == message.GetType());
                if (eventHandler != null)
                {
                    eventsToReceive--;
                    RequestEventsToRestore();

                    Context.System.Log.Debug("Actor {0}: Restore Event: {1}", Self.Path.Name, message);

                    eventHandler.Action(message);
                }
                else
                {
                    Context.System.Log.Debug("Actor {0} during restore stash command: {1}", Self.Path.Name, message);

                    Stash.Stash();
                }
            }
        }

        private void StartRestoring()
        {
            Context.System.Log.Debug("Actor {0}: StartRestore", Self.Path.Name);
            IsRestoring = true;
            BecomeStacked(Restoring);

            eventStore.Tell(new StartRestore(GenerateInterestingEvents()));

            eventsToReceive = 0;
            RequestEventsToRestore();
        }

        private void RequestEventsToRestore()
        {
            if (eventsToReceive < BufferLowLimit)
            {
                eventsToReceive += NrRestoreEvents;
                eventStore.Tell(new RestoreEvents(NrRestoreEvents));
            }
        }
        #endregion
    }

    /// <summary>
    /// An actor with persisted state with an Id
    /// </summary>
    public abstract class DurableActor<TIndex> : DurableActor
    {
        public TIndex Id { get; private set; }

        protected DurableActor(IActorRef eventStore, TIndex id) : base(eventStore)
        {
            Id = id;
        }

        protected override InterestingEvents GenerateInterestingEvents()
        {
            return new InterestingEvents<TIndex>(Id, events.Select(e => e.Type));
        }
    }
}
