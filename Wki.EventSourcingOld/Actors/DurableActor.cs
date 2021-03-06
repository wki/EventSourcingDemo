﻿using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Util;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Protocol.Statistics;
using Wki.EventSourcing.Protocol.LifeCycle;
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
        private DurableActorStatistics statistics;

        // after this timestamp an actor of this type will assk for passivation
        protected TimeSpan passivationTimeSpan;

        // simpler access to current status
        protected bool IsRestoring => statistics.IsRestoring;
        protected bool IsOperating => statistics.IsOperating;

        // keep all events and commands we may respond to
        protected Dictionary<Type, Handler> commands;
        protected Dictionary<Type, Handler> events;


        // we must know our event store. Typically the office will tell us
        private readonly IActorRef eventStore;

        // during restore count events for aquiring next junk before completion
        // we keep track of the nr of events we still await to receive
        private int eventsToReceive;

        protected DurableActor(IActorRef eventStore)
        {
            this.eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));

            passivationTimeSpan = TimeSpan.MaxValue;

            commands = new Dictionary<Type, Handler>();
            events = new Dictionary<Type, Handler>();

            statistics = new DurableActorStatistics();

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
            eventStore.Tell(new Unsubscribe());
        }

        protected virtual InterestingEvents GenerateInterestingEvents() =>
            new InterestingEvents(events.Values.Select(e => e.Type));

        #region Event and Command Handling
        /// <summary>
        /// Declare an event to handle
        /// </summary>
        /// <param name="eventHandler">Event handler.</param>
        /// <typeparam name="E">The 1st type parameter.</typeparam>
        protected void Recover<E>(Action<E> eventHandler) =>
            events.Add(typeof(E), new Handler(typeof(E), e => eventHandler((E)e)));

        /// <summary>
        /// Receive an event (alias to command)
        /// </summary>
        /// <param name="eventHandler">Event handler.</param>
        /// <typeparam name="E">Type of the event to receive</typeparam>
        protected void Receive<E>(Action<E> eventHandler) =>
            commands.Add(typeof(E), new Handler(typeof(E), c => eventHandler((E)c)));

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
        private void StartOperating()
        {
            UnbecomeStacked();
            Stash.UnstashAll();

            Context.System.Scheduler.ScheduleTellRepeatedly(
                initialDelay: ActorStillAliveInterval,
                interval: ActorStillAliveInterval,
                receiver: Self,
                message: new Tick(),
                sender: Self
            );
        }

        /// <summary>
        /// Regular Operation: receive Commands and Events
        /// </summary>
        /// <param name="message">Message.</param>
        protected override void OnReceive(object message)
        {
            var messageType = message.GetType();

            switch(message)
            {
                case Tick _:
                    HandleTick();
                    break;

                case GetStatistics _:
                    Sender.Tell(statistics);
                    break;

                case Event e when (events.ContainsKey(messageType)):
                    statistics.EventReceived();
                    events[messageType].Handle(e);
                    break;

                case Object c when (commands.ContainsKey(messageType)):
                    statistics.CommandReceived();
                    commands[messageType].Handle(c);
                    break;

                default:
                    statistics.UnhandledMessageReceived();
                    Unhandled(message);
                    break;
            }


            //if (message is Tick)
            //    HandleTick();
            //else if (message is GetStatistics)
            //    Sender.Tell(statistics);
            //else
            //{
            //    var messageType = message.GetType();
            //    var eventHandler = events.Find(e => e.Type == messageType);
            //    if (eventHandler != null)
            //    {
            //        statistics.EventReceived();
            //        eventHandler.Action(message);
            //    }
            //    else
            //    {
            //        var commandHandler = commands.Find(c => c.Type == messageType);
            //        if (commandHandler != null)
            //        {
            //            statistics.CommandReceived();
            //            commandHandler.Action(message);
            //        }
            //        else
            //        {
            //            statistics.UnhandledMessageReceived();
            //            Unhandled(message);
            //        }
            //    }
            //}
        }

        private void HandleTick()
        {
            //Console.WriteLine("Parent:{0}", Context.Parent.Path);
            if (ShouldPassivate())
            {
                statistics.PassivateSent();
                Context.Parent.Tell(new Passivate());
            }
            else
            {
                statistics.StillAliveSent();
                Context.Parent.Tell(new StillAlive());
            }
        }

        private bool ShouldPassivate()
        {
            var passivationTime = statistics.LastActivity + passivationTimeSpan;

            //Console.WriteLine(
            //    "Now: {0}, started: {1}, LastActivity: {2}, Passivation: {3}", 
            //    SystemTime.Now, statistics.StartedAt, statistics.LastActivity, passivationTime
            //);
            return passivationTimeSpan == TimeSpan.MaxValue
                // never passivate in case of infinite timespan
                ? false
                // passivate if we idled longer than passivation timespan
                : SystemTime.Now > passivationTime;
        }
        #endregion

        #region restore state
        /// <summary>
        /// Behaviour during restore: Receive events but stash other messages
        /// </summary>
        /// <param name="message">Message.</param>
        private void Restoring(object message)
        {
            var messageType = message.GetType();

            switch(message)
            {
                case EndOfTransmission _:
                    statistics.ChangeStatus(DurableActorStatus.Operating);

                    Context.System.Log.Info(
                        "Actor {0}: CompletedRestore {1} Events, {2:N3}s",
                        Self.Path.Name,
                        statistics.NrRestoreEvents,
                        statistics.RestoreDuration.TotalSeconds
                    );

                    StartOperating();
                    break;

                case GetStatistics _:
                    Sender.Tell(statistics);
                    break;

                case Event e when (events.ContainsKey(messageType)):
                    eventsToReceive--;
                    RequestEventsToRestore();

                    Context.System.Log.Debug("Actor {0}: Restore Event: {1}", Self.Path.Name, message);

                    statistics.EventReceived();

                    try
                    {
                        events[messageType].Handle(message);
                    }
                    catch (Exception ex)
                    {
                        // if we die during load this will repeat next time
                        Context.Parent.Tell(new DiedDuringRestore());

                        Context.System.Log.Error("Actor {0}: Died during Restore Event {1} with {2}", Self.Path.Name, message, ex);
                        throw new ActorInitializationException(Self, $"Died during Restore Event {message}", ex);
                    }
                    break;

                default:
                    Context.System.Log.Debug("Actor {0} during restore stash command: {1}", Self.Path.Name, message);

                    statistics.CommandReceived();
                    Stash.Stash();
                    break;
            }
            //if (message is EndOfTransmission)
            //{
            //    statistics.ChangeStatus(DurableActorStatus.Operating);

            //    Context.System.Log.Info(
            //        "Actor {0}: CompletedRestore {1} Events, {2:N3}s",
            //        Self.Path.Name,
            //        statistics.NrRestoreEvents,
            //        statistics.RestoreDuration.TotalSeconds
            //    );

            //    StartOperating();
            //}
            //else if (message is GetStatistics)
            //    Sender.Tell(statistics);
            //else
            //{
            //    var eventHandler = events.Find(e => e.Type == message.GetType());
            //    if (eventHandler != null)
            //    {
            //        eventsToReceive--;
            //        RequestEventsToRestore();

            //        Context.System.Log.Debug("Actor {0}: Restore Event: {1}", Self.Path.Name, message);

            //        statistics.EventReceived();

            //        try
            //        {
            //            eventHandler.Action(message);
            //        }
            //        catch (Exception e)
            //        {
            //            // if we die during load this will repeat next time
            //            Context.Parent.Tell(new DiedDuringRestore());

            //            Context.System.Log.Error("Actor {0}: Died during Restore Event {1} with {2}", Self.Path.Name, message, e);
            //            throw new ActorInitializationException(Self, $"Died during Restore Event {message}", e);
            //        }
            //    }
            //    else
            //    {
            //        Context.System.Log.Debug("Actor {0} during restore stash command: {1}", Self.Path.Name, message);

            //        statistics.CommandReceived();

            //        Stash.Stash();
            //    }
            //}
        }

        private void StartRestoring()
        {
            Context.System.Log.Debug("Actor {0}: StartRestore", Self.Path.Name);
            statistics.ChangeStatus(DurableActorStatus.Restoring);
            BecomeStacked(Restoring);

            eventStore.Tell(new StartRestore());

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
            return new InterestingEvents<TIndex>(Id, events.Values.Select(e => e.Type));
        }
    }
}
