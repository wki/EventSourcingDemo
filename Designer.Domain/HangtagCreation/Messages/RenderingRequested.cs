using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class RenderingRequested : Event<int>
    {
        public RenderingRequested(int id) : base(id)
        {
        }
    }
}
