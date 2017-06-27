using Akka.TestKit.NUnit;
using System.Collections.Generic;

namespace Wki.EventSourcing.Tests
{
    public partial class DurableRetrievalTest : TestKit
    {
        // a simple aggregate root
        public class DurableState : IState<DurableState>
        {
            #region Commands
            public class Command : ICommand { }
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

            public IState<DurableState> Apply(IEvent @event)
            {
                AppliedEvents.Add(@event.GetType().Name);
                return this;
            }
        }
    }
}
