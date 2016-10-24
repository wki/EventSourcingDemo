using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class AddLanguage : DispatchableCommand<int>
    {
        public AddLanguage(int id) : base(id)
        {
        }
    }
}
