using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class UpdateAddress : DispatchableCommand<int>
    {
        public UpdateAddress(int id) : base(id)
        {
        }
    }
}
