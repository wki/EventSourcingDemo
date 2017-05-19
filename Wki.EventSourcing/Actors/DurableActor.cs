using System;
using Akka.Actor;
using Wki.EventSourcing.Infrastructure;
using Wki.EventSourcing.Protocol;
using static Wki.EventSourcing.Util.Constant;
using Akka.Util;
using Wki.EventSourcing.Protocol.Statistics;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Base class for a durable actor
    /// </summary>
    public abstract class DurableActor: UntypedActor, IWithUnboundedStash
    {
        public IActorRef EventStore;
        public string PersistenceId;
        public int LastEventPos;
        public IActorRef LastCommandSender;
        public TimeSpan DefaultReceiveTimeout;
        public DurableActorStatistics Statistics;

        public IStash Stash { get; set; }

        public DurableActor(IActorRef eventStore)
        {
            EventStore = eventStore;
            PersistenceId = this.GetType().Name;
            LastEventPos = -1;
            LastCommandSender = null;
            DefaultReceiveTimeout = MaxActorIdleTimeSpan;
            Statistics = new DurableActorStatistics();
        }

        protected override void PreStart()
        {
            base.PreStart();
            EventStore.Tell(LoadNextEvents.StartingAt(LastEventPos));
            SetReceiveTimeout(MaxRestoreIdleTimeSpan);
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
            LastEventPos = eventRecord.Id;
            Apply(eventRecord.Event);
        }

        /// <summary>
        /// To be implemented by each actor implementation
        /// </summary>
        /// <param name="e"></param>
        public abstract void Apply(DomainEvent e);

        /// <summary>
        /// Persist a given domain Event to the event Store
        /// </summary>
        /// <param name="domainEvent"></param>
        public void Persist(DomainEvent domainEvent)
        {
            Statistics.EventPersisted();
            LastCommandSender = Sender;
            EventStore.Tell(new Persist(domainEvent, PersistenceId));
            SetReceiveTimeout(MaxPersistIdleTimeSpan);
            BecomeStacked(Persisting);
        }

        // Loading behavior
        public void Loading(object message)
        {
            switch(message)
            {
                case ReceiveTimeout _:
                    throw new PersistException("Timeout reached during Load");

                case EventRecord r:
                    HandleEventRecord(r);
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
                    throw new PersistException(error);

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
    /// Base class for a durable actor with an Index
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    public abstract class DurableActor<TIndex>: DurableActor
    {
        public TIndex id;

        public DurableActor(IActorRef eventStore, TIndex id)
            :base(eventStore)
        {
            this.id = id;
            PersistenceId = $"{this.GetType().Name}-{id}";
        }
    }

    
    /// <summary>
    /// Base class for a durable actor with index and state
    /// </summary>
    /// <typeparam name="TIndex"></typeparam>
    /// <typeparam name="TState"></typeparam>
    /// <example>
    /// public class Xxx : DurableActor&lt;int, XxxState&gt;
    /// {
    ///     public Xxx(int id) : base(id)
    ///     {
    ///     // TODO: Subscribe auf alle Events für PersistenceId
    ///     }
    /// 
    ///     #region commands with a local base class (allow matching via Xxx.Command)
    ///     public class Command : DispatchableCommand<int>
    ///     {
    ///         public Command(int id) : base(id) { }
    ///     }
    ///     
    ///     public class DoSomething : Command
    ///     {
    ///         public string Thing { get; private set; }
    ///     
    ///         public DoSomething(int id, string thing) : base(id)
    ///         {
    ///             Thing = thing;
    ///         }
    ///     }
    ///     #endregion
    ///     
    ///     protected override void OnReceive(object message)
    ///     {
    ///         switch (message)
    ///         {
    ///             case DoSomething d:
    ///                 Persist(new XxxState.SomethingDone(42, "foo"));
    ///                 break;
    ///         }
    ///     }
    ///     
    ///     // only BuildInitialState() is needed, Apply() has a default implementation
    ///     protected override State<XxxState> BuildInitialState() =>
    ///         new XxxState(Id, "foo");
    /// }
    /// </example>
    public abstract class DurableActor<TIndex, TState>: DurableActor<TIndex>
    {
        public State<TState> state;

        public DurableActor(IActorRef eventStore, TIndex id) 
            : base(eventStore, id)
        {
            state = BuildInitialState();
        }

        /// <summary>
        /// initially construct the state
        /// </summary>
        /// <returns></returns>
        protected abstract State<TState> BuildInitialState();

        /// <summary>
        /// Default implementation: let the state comsume the event
        /// </summary>
        /// <param name="e"></param>
        override public void Apply(DomainEvent e) =>
            state = state.Update(e);

    }
}
