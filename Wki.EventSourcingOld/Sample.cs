// Demonstrieren der Anwendungsmöglichkeiten
// Durable Actors -- Basisklassen


/*
 
Subscriptions:
 - Event bzw. Basisklasse
 - Persistence-Id
 - Start-Index
 - Angabe ob Event oder Envelope gewünscht / macht das Sinn?
 - alternativ: immer Envelope liefern

Events -> Aktor:
 - Envelopes mit Id und Metadaten notwendig für Process Mgr für Hot-Subscribe

 */


using System;

namespace Wki.EventSourcing.Infrastructure
{
    public class DispatchableCommand<TIndex>
    {
        public TIndex Id { get; private set; }

        public DispatchableCommand(TIndex id)
        {
            Id = id;
        }
    }

    public class DomainEvent { }
    public class DomainEvent<TIndex>: DomainEvent
    {
        public TIndex Id { get; private set; }

        public DomainEvent(TIndex id)
        {
            Id = id;
        }
    }

    // Basisklasse für XxxState
    abstract public class State<TState>
    {
        public abstract State<TState> Update(DomainEvent e);
    }

    // EventRecords are handed over to durable actors as part of the subscription
    public class EventRecord
    {
        public int Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string AggregateId { get; private set; }
        public string AggregateType { get; private set; }
        public string PersistenceId { get => $"{AggregateType}-{AggregateId}"; }
        public DomainEvent Event { get; private set; }
    }
}

namespace Wki.EventSourcing.BaseActors
{
    using Akka.Actor;
    using Wki.EventSourcing.Infrastructure;

    #region durable actors
    public abstract class DurableActor : UntypedActor
    {
        // TODO: EventStore
        public string PersistenceId { get; set; }

        public DurableActor()
        {
            PersistenceId = this.GetType().Name;
        }

        public abstract void Apply(DomainEvent e);
        public void Persist(DomainEvent d)
        {
        }
    }

    public abstract class DurableActor<TIndex>: DurableActor
    {
        public TIndex Id { get; set; }

        public DurableActor(TIndex id)
        {
            Id = id;
            PersistenceId = $"{this.GetType().Name}-{id}";
        }
    }

    public abstract class DurableActor<TIndex, TState>: DurableActor<TIndex>
    {
        public State<TState> State { get; set; }

        public DurableActor(TIndex id): base(id)
        {
            State = BuildInitialState();
        }

        protected abstract State<TState> BuildInitialState();

        override public void Apply(DomainEvent e) =>
            State = State.Update(e);

    }
    #endregion

    #region office actors
    public abstract class OfficeActor: UntypedActor
    {
    }

    public abstract class OfficeActor<TDurable>: OfficeActor
    {

    }
    #endregion

}


// Durable Actors -- Beispiel State
namespace MyDomain.MySubdomain
{
    using Akka.Actor;
    using System;
    using Wki.EventSourcing.BaseActors;
    using Wki.EventSourcing.Infrastructure;

    public class XxxState : State<XxxState>
    {
        // commands beim Actor

        #region events
        public class Event : DomainEvent<int>
        {
            public Event(int id) : base(id) { }
        }

        public class SomethingDone : Event
        {
            public string Foo { get; private set; }
            public SomethingDone(int id, string foo) : base(id)
            {
                Foo = foo;
            }
        }
        #endregion

        public int Id { get; private set; }
        public string Foo { get; private set; }

        public XxxState(int id, string foo)
        {
            Id = id;
            Foo = foo;
        }

        public override State<XxxState> Update(DomainEvent e)
        {
            switch (e)
            {
                case SomethingDone s:
                    return new XxxState(Id, s.Foo);

                default:
                    return this;
            }
        }
    }

    // ein einfaches Actor Beispiel
    public class Xxx : DurableActor<int, XxxState>
    {
        public Xxx(int id) : base(id)
        {
            // TODO: Subscribe auf alle Events für PersistenceId
        }

        #region commands
        public class Command : DispatchableCommand<int>
        {
            public Command(int id) : base(id) { }
        }

        public class DoSomething : Command
        {
            public string Thing { get; private set; }

            public DoSomething(int id, string thing) : base(id)
            {
                Thing = thing;
            }
        }
        #endregion

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case DoSomething d:
                    Persist(new XxxState.SomethingDone(42, "foo"));
                    break;
            }
        }

        protected override State<XxxState> BuildInitialState() =>
            new XxxState(Id, "foo");
    }

    public class XxxOffice: OfficeActor<Xxx>
    {
        // kein Subscribe. Commands kommen an.

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case Xxx.Command cmd:
                    // instantiate Xxx Actor and forward to it
                    break;

                case "whatever":
                    // besondere Dinge
                    break;

                default:
                    break;
            }
        }
    }

    // Coordination Office für den Process Manager XxxCompletion
    public class XxxCompletionOffice : OfficeActor
    {
        public IActorRef EventStore { get; set; }

        // muss auf Prozess-Start Signal lauschen
        public XxxCompletionOffice()
        {
            // TODO: subscribe for Trigger Event
            EventStore.Tell("Subscribe ...");
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case XxxState.SomethingDone s:
                    // instantiate Process with s.Id
                    break;
            }
        }
    }

    // Process Manager für den XxxCompletion Prozess. Wird durch das Office gestartet
    public class XxxCompletion : DurableActor<int>
    {
        public XxxCompletion(int id) : base(id)
        {
            // Subscribe auf ... Ereignisse seit der ID des Trigger Ereignisses
        }

        public override void Apply(DomainEvent e)
        {
            throw new NotImplementedException();
        }

        protected override void OnReceive(object message)
        {
            throw new NotImplementedException();
        }
    }

    // View für globale Informationen
    public class ToDoList : DurableActor
    {
        public ToDoList()
        {
            // TODO: subscribe for what we want to see...
        }

        public override void Apply(DomainEvent e)
        {
            throw new NotImplementedException();
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case DomainEvent e:
                    Apply(e);
                    break;
            }
        }
    }
}