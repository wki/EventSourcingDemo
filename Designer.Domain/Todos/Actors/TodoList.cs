using System;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.Todos.Actors
{
    public class TodoList : DurableActor<int>
    {
        public TodoList(int id) : base(id)
        {
            // subscribe hangtag list changes

            // recover person <id> changes
        }
    }
}
