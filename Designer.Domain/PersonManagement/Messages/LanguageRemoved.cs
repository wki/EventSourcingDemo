using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class LanguageRemoved : Event<int>
    {
        public LanguageRemoved(int id) : base(id)
        {
        }
    }
}
