using Wki.EventSourcing.Protocol;

namespace Designer.Domain.PersonManagement.Messages
{
    public class LanguageRemoved : Event<int>
    {
        public LanguageRemoved(int id) : base(id)
        {
        }
    }
}
