using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class RenderingCompleted : Event<int>
    {
        public RenderingCompleted(int id) : base(id)
        {
        }
    }
}
