using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class LanguageAdded : Event<int>
    {
        public LanguageAdded(int id) : base(id)
        {
        }
    }
}
