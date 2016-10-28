using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.Todos.Actors
{
    public class TodoListOffice : OfficeActor<TodoList, int>
    {
        public TodoListOffice(IActorRef eventStore) : base(eventStore)
        {
        }
    }
}
