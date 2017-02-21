using Wki.EventSourcing.Protocol;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class RenderingCompleted : Event<int>
    {
        public RenderingCompleted(int id) : base(id)
        {
        }
    }
}
