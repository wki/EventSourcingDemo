using Wki.EventSourcing.Protocol;

namespace Designer.Domain.HangtagCreation.Messages
{
    public class HangtagCloned : Event<int>
    {
        public HangtagCloned(int id) : base(id)
        {
        }
    }
}
