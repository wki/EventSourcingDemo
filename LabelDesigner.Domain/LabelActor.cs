using Akka.Actor;
using System;
using Wki.EventSourcing;
using Wki.EventSourcing.Actors;
using System.Collections.Generic;

namespace LabelDesigner.Domain
{
    /// <summary>
    /// Label aggregate root
    /// </summary>
    public class LabelActor : DurableActor<Label, int>
    {
        public LabelActor(IActorRef eventStore, int id) : base(eventStore, id)
        {
        }

        protected override EventFilter BuildEventFilter() =>
            WantEvents
                .ForPersistenceId(PersistenceId)
                .When<Label.Event>();

        protected override Label BuildInitialState() =>
            new Label(Id);

        //protected override void Handle(object message)
        //{
        //    // falls es noch Nachrichten gibt...
        //    // Commands und Events sind schon berücksichtigt
        //}
    }
}
