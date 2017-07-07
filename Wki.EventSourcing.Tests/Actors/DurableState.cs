using System;
using System.Collections.Generic;

namespace Wki.EventSourcing.Tests.Actors
{
    // a simple aggregate root
    public class DurableState : IState<DurableState>
    {
        #region Queries
        public class GetState { }
        #endregion
        
        #region Commands
        public class Command : ICommand { }
        public class LetSomethingHappen: ICommand { }
        public class HandleFoo : ICommand { }
        #endregion

        #region Events
        public class Event : IEvent { }
        public class SomethingHappened : Event { }
        public class FooHandled : Event { }
        #endregion

        public int Id { get; private set; }
        public List<string> AppliedEvents { get; private set; } = new List<string>();

        // default ctor needed
        public DurableState() { }

        public DurableState(int id)
        {
            Id = id;
        }

        public DurableState ApplyEvent(IEvent @event)
        {
            AppliedEvents.Add(@event.GetType().Name);
            return this;
        }

        public IEvent HandleCommand(ICommand command)
        {
            switch(command)
            {
                case LetSomethingHappen _:
                    return new SomethingHappened();

                case HandleFoo _:
                    return new FooHandled();

                default:
                    return null;
            }
        }
    }
}
