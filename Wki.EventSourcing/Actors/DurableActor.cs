using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;

namespace Wki.EventSourcing.Actors
{
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

        // during restore count events for aquiring next junk before completion
        private const int NrRestoreEvents = 100;    // Size of a block of events to request
        private const int BufferLowLimit = 10;      // if we are expecting less than this -- re-request!
        private int eventsToReceive;                // events still awaiting to receive

        public DurableActor()
        {
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

        private void StartRestoring()
        {
            Context.System.Log.Debug("Actor {0}: StartRestore", Self.Path.Name);
            IsRestoring = true;
            BecomeStacked(Restoring);

            Context.EventStore().Tell(new StartRestore(GenerateInterestingEvents()));

            eventsToReceive = 0;
            RequestEventsToRestore();
        }

        protected virtual InterestingEvents GenerateInterestingEvents()
        {
            return new InterestingEvents(events.Select(e => e.Type));
        }

        private void RequestEventsToRestore()
        {
            if (eventsToReceive < BufferLowLimit)
            {
                eventsToReceive += NrRestoreEvents;
                Context.EventStore().Tell(new RestoreEvents(NrRestoreEvents));
            }
        }

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

            Context.EventStore().Tell(new PersistEvent(@event));
        }

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

        /// <summary>
        /// Behaviour during restore.
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
    }

    public abstract class DurableActor<TIndex> : DurableActor
    {
        public TIndex Id { get; private set; }

        public DurableActor(TIndex id)
        {
            Id = id;
        }

        protected override InterestingEvents GenerateInterestingEvents()
        {
            return new InterestingEvents<TIndex>(Id, events.Select(e => e.Type));
        }
    }
}
