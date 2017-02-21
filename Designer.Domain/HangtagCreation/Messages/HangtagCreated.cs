using System;
using Wki.EventSourcing.Protocol;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class HangtagCreated : Event<int>
    {
        public HangtagCreated(int id) : base(id)
        {
        }
    }
}
