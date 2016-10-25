using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class HangtagCloned : Event<int>
    {
        public HangtagCloned(int id) : base(id)
        {
        }
    }
}
