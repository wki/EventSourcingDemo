using Wki.EventSourcing.Protocol;

namespace Designer.Domain.PersonManagement.Messages
{
    public class LanguageAdded : Event<int>
    {
        public LanguageAdded(int id) : base(id)
        {
        }
    }
}
