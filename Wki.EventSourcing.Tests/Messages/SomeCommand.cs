using System;
using Wki.EventSourcing.Messages;

namespace Wki.EventSourcing.Tests.Messages
{
    public class SomeCommand : DispatchableCommand<int>
    {
        public SomeCommand(int id) : base(id)
        {
        }
    }
}
