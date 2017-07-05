using System;
using Wki.EventSourcing;

namespace LabelDesigner.Domain
{
    /// <summary>
    /// Persistent state with event processing logic
    /// </summary>
    public class Label : IState<Label>
    {
        #region commands
        public class Command : ICommand<int>
        {
            public int Id { get; protected set; }
        }

        public class ChangeName : Command
        {
            public string Name { get; private set; }

            public ChangeName(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        #endregion

        #region events
        public class Event : IEvent<int>
        {
            public int Id { get; protected set; }
        }

        public class NameChanged: Event
        {
            public string Name { get; private set; }

            public NameChanged(int id, string name)
            {
                Id = id;
                Name = name;
            }
        }
        #endregion

        public int Id { get; private set; }
        public string Name { get; private set; }

        public Label(): this(0) { }
        
        public Label(int id, string name = null)
        {
            Id = id;
        }

        private Label WithName(string name) =>
            new Label(Id, name);

        public Label ApplyEvent(IEvent @event)
        {
            switch(@event)
            {
                case NameChanged nameChanged:
                    return WithName(nameChanged.Name);

                default:
                    throw new ArgumentException($"cannot handle event of type '{@event.GetType().FullName}'");
            }
        }

        public IEvent HandleCommand(ICommand command)
        {
            switch(command)
            {
                case ChangeName changeName:
                    return new NameChanged(Id, changeName.Name);

                default:
                    throw new ArgumentException($"cannot handle command of type '{command.GetType().FullName}'");
            }
        }
    }
}
