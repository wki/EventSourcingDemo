using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Util;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.LiveCycle;
using Wki.EventSourcing.Protocol.Subscription;
using Wki.EventSourcing.Protocol.EventStore;
using Wki.EventSourcing.Protocol.Misc;
using static Wki.EventSourcing.Util.Constant;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// An actor with persisted state without an Id
    /// </summary>
    /// <description>
    /// supported protocols:
    ///  * subscribe at event store
    ///  * reconstitute from event store
    ///  * persist events
    ///  * graceful passivation
    ///    - default for non-id durable actors: do not passivate
    ///    - default for with-id durable actors: passivate
    /// </description>
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
    /// </example>
    public abstract class DurableActor : UntypedActor, IWithUnboundedStash
    {
        /// <summary>
        /// Stash for unhandled messages. Will be processed after restore
        /// </summary>
        /// <value>The stash.</value>
        public IStash Stash { get; set; }

        // maintain a complete state (for knowledge and statistics)
        private DurableActorStatistics durableActorStatistics;

        // after this timestamp an actor of this type will assk for passivation
        protected TimeSpan passivationTimeSpan;

        // simpler access to current status
        protected bool IsRestoring => durableActorStatistics.IsRestoring;
        protected bool IsOperating => durableActorStatistics.IsOperating;

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

            passivationTimeSpan = TimeSpan.MaxValue;

            commands = new List<Handler>();
            events = new List<Handler>();

            durableActorStatistics = new DurableActorStatistics();

            Context.System.Log.Info("start Building {0}", Self.Path.ToString());
        }

        protected override void PreStart()
        {
            base.PreStart();
            eventStore.Tell(new Subscribe(GenerateInterestingEvents()));
            StartRestoring();
        }

        protected override void PostRestart(Exception reason)
        {
            base.PostRestart(reason);
            StartRestoring();
        }

        protected override void PostStop()
        {
            base.PostStop();
            SetReceiveTimeout(null);
            eventStore.Tell(new Unsubscribe());
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
            if (message is Tick)
                HandleTick();
            else if (message is GetStatistics)
                Sender.Tell(durableActorStatistics);
            else
            {
                var now = SystemTime.Now;

                var eventHandler = events.Find(e => e.Type == message.GetType());
                if (eventHandler != null)
                {
                    durableActorStatistics.EventReceived();
                    eventHandler.Action(message);
                }
                else
                {
                    var commandHandler = commands.Find(c => c.Type == message.GetType());
                    if (commandHandler != null)
                    {
                        durableActorStatistics.CommandReceived();
                        commandHandler.Action(message);
                    }
                    else
                    {
                        durableActorStatistics.UnhandledMessageReceived();
                        Unhandled(message);
                    }
                }
            }
        }

        private void HandleTick()
        {
            if (durableActorStatistics.ShouldPassivate())
                Context.Parent.Tell(new Passivate());
            else
            {
                durableActorStatistics.StillAliveSent();
                eventStore.Tell(new StillAlive());
            }
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
                durableActorStatistics.ChangeStatus(DurableActorStatus.Operating);

                Context.System.Log.Info(
                    "Actor {0}: CompletedRestore {1} Events, {2:N3}s",
                    Self.Path.Name,
                    durableActorStatistics.NrRestoreEvents,
                    durableActorStatistics.RestoreDuration.TotalSeconds
                );

                Context.System.Scheduler.ScheduleTellRepeatedly(
                    initialDelay: ActorStillAliveInterval,
                    interval: ActorStillAliveInterval,
                    receiver: Self,
                    message: new Tick(),
                    sender: Self
                );

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

                    durableActorStatistics.EventReceived();

                    eventHandler.Action(message);
                }
                else
                {
                    Context.System.Log.Debug("Actor {0} during restore stash command: {1}", Self.Path.Name, message);

                    durableActorStatistics.CommandReceived();

                    Stash.Stash();
                }
            }
        }

        private void StartRestoring()
        {
            Context.System.Log.Debug("Actor {0}: StartRestore", Self.Path.Name);
            durableActorStatistics.ChangeStatus(DurableActorStatus.Restoring);
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
                eventStore.Tell(new RestoreNextEvents(NrRestoreEvents));
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
            passivationTimeSpan = MaxActorIdleTimeSpan;
        }

        protected override InterestingEvents GenerateInterestingEvents()
        {
            return new InterestingEvents<TIndex>(Id, events.Select(e => e.Type));
        }
    }
}
