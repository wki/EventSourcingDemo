using System;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.Todos.Actors
{
    public class TodoListOffice : OfficeActor<TodoList, int>
    {
        public TodoListOffice()
        {
        }
    }
}
