using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class RemoveLanguage : DispatchableCommand<int>
    {
        public RemoveLanguage(int id) : base(id)
        {
        }
    }
}
