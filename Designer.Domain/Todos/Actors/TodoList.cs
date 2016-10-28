using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.Todos.Actors
{
    public class TodoList : DurableActor<int>
    {
        public TodoList(IActorRef eventStore, int id) : base(eventStore, id)
        {
            // subscribe hangtag list changes

            // recover person <id> changes
        }
    }
}
