using Wki.EventSourcing.Protocol;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class RenderingRequested : Event<int>
    {
        public RenderingRequested(int id) : base(id)
        {
        }
    }
}
