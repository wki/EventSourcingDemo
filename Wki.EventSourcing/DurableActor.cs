using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Statistics;
using static Wki.EventSourcing.Util.Constant;
using Wki.EventSourcing.Protocol.Load;
using Wki.EventSourcing.Protocol.Persistence;
using Wki.EventSourcing.Persistence;
using Wki.EventSourcing.Protocol.Subscription;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Base class for a durable actor
    /// </summary>
    public abstract class DurableActor: UntypedActor, IWithUnboundedStash
    {
        public IActorRef EventStore;
        public string PersistenceId;
        public bool HasState;
        public int LastEventId;
        public IActorRef LastCommandSender;
        public TimeSpan DefaultReceiveTimeout;
        public DurableActorStatistics Statistics;

        public IStash Stash { get; set; }

        public DurableActor(IActorRef eventStore)
        {
            EventStore = eventStore;
            PersistenceId = GetType().Name;
            HasState = false;
            LastEventId = -1;
            LastCommandSender = null;
            DefaultReceiveTimeout = MaxActorIdleTimeSpan;
            Statistics = new DurableActorStatistics();
        }

        protected void Subscribe() =>
            Subscribe(BuildEventFilter());

        protected void Subscribe(EventFilter eventFilter) =>
            EventStore.Tell(new Subscribe(eventFilter));

        protected void UnSubscribe() =>
            EventStore.Tell(Unsubscribe.Instance);

        // must be overloaded in order to return events we are interested in
        protected abstract EventFilter BuildEventFilter();

        protected override void PreStart()
        {
            base.PreStart();
            EventStore.Tell(new LoadSnapshot(PersistenceId));
            SetReceiveTimeout(MaxRestoreIdleTimeSpan);
            if (HasState)
                BecomeStacked(WaitingForSnapshot);
            else
                BecomeStacked(Loading);
            Statistics.StartRestoring();
        }

        /// <summary>
        /// universal handling of an eventRecord received from event Store
        /// </summary>
        /// <param name="eventRecord"></param>
        protected void HandleEventRecord(EventRecord eventRecord)
        {
            Statistics.EventReceived();
            LastEventId = eventRecord.Id;
            Apply(eventRecord.Event);
        }

        /// <summary>
        /// To be implemented by each actor implementation: Apply an event
        /// </summary>
        /// <param name="e"></param>
        protected abstract void Apply(IEvent e);

        /// <summary>
        /// Persist a given domain Event to the event Store
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void Persist(IEvent domainEvent)
        {
            Statistics.EventPersisted();
            LastCommandSender = Sender;
            EventStore.Tell(new PersistEvent(PersistenceId, domainEvent));
            SetReceiveTimeout(MaxPersistIdleTimeSpan);
            BecomeStacked(Persisting);
        }

        /// <summary>
        /// Save the state obtained state. default behavior: do nothing
        /// </summary>
        /// <param name="state"></param>
        virtual protected void SaveSnapshot(Snapshot snapshot) { }

        // Wait for Snapshot behavior
        protected virtual void WaitingForSnapshot(object message)
        {
            switch(message)
            {
                case ReceiveTimeout _:
                case NoSnapshot _:
                    // switch to Loading
                    break;

                case Snapshot snapshot:
                    SaveSnapshot(snapshot);
                    break;
            }

            EventStore.Tell(LoadNextEvents.After(LastEventId));
            Become(Loading);
        }

        // Loading behavior
        public void Loading(object message)
        {
            switch(message)
            {
                case ReceiveTimeout _:
                    throw new PersistTimeoutException("Timeout reached during Load");

                case EventRecord r:
                    HandleEventRecord(r);
                    // TODO: nach 1000 Messages ist Schluss... 
                    break;

                case End _:
                    Statistics.FinishedRestoring();
                    SetReceiveTimeout(DefaultReceiveTimeout);
                    UnbecomeStacked();
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }

        // Persisting behavior
        public void Persisting(object message)
        {
            switch(message)
            {
                case ReceiveTimeout _:
                    var error = "Timeout during persisting";
                    LastCommandSender.Tell(Reply.Error(error));
                    throw new PersistTimeoutException(error);

                case EventRecord r:
                    HandleEventRecord(r);
                    UnbecomeStacked();
                    break;

                default:
                    Stash.Stash();
                    break;
            }
        }
    }

    /// <summary>
    /// Base class for a durable actor with index and state
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <typeparam name="TIndex"></typeparam>
    /// <example>
    /// // Xxx: Aggregate Root, Xxx.Event, Xxx.Command: Basis Klassen
    /// public class XxxClerk : DurableActor&lt;int, Xxx&gt;
    /// {
    ///     public XxxClerk(int id) : base(id)
    ///     {
    ///     // TODO: Subscribe auf alle Events für PersistenceId
    ///     }
    /// 
    ///     protected override void OnReceive(object message)
    ///     {
    ///         switch (message)
    ///         {
    ///             // one example command handler
    ///             case DoSomething d:
    ///                 Persist(new Xxx.SomethingDone(42, "foo"));
    ///                 break;
    ///             
    ///             // handle all events
    ///             case Xxx.Event e:
    ///                 Apply(e);
    ///                 break;
    ///         }
    ///     }
    ///     
    ///     // only BuildInitialState() is needed, Apply() has a default implementation
    ///     protected override IState<XxxState> BuildInitialState() =>
    ///         new XxxState(Id, "foo");
    /// }
    /// </example>
    public abstract class DurableActor<TState>: DurableActor
        where TState: IState<TState>
    {
        public IState<TState> State;

        public DurableActor(IActorRef eventStore) 
            : base(eventStore)
        {
            State = BuildInitialState();
        }

        /// <summary>
        /// initially construct the state
        /// </summary>
        /// <returns></returns>
        protected abstract TState BuildInitialState();

        /// <summary>
        /// Default implementation: let the state consume the event
        /// </summary>
        /// <param name="event"></param>
        override protected void Apply(IEvent @event) =>
            State = State.Apply(@event);

        protected override void SaveSnapshot(Snapshot snapshot)
        {
            if (snapshot.State is TState state)
            {
                State = state;
                LastEventId = snapshot.LastEventId;
            }
        }
    }

    /// <summary>
    /// Base class for a durable actor with a State and an index
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public abstract class DurableActor<TState, TIndex> : DurableActor<TState>
        where TState: IState<TState>
    {
        public TIndex Id;

        public DurableActor(IActorRef eventStore, TIndex id)
            : base(eventStore)
        {
            Id = id;
            PersistenceId = $"{this.GetType().Name}-{id}";
        }
    }
}
