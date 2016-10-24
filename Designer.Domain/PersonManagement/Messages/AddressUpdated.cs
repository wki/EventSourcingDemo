using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class AddressUpdated : Event<int>
    {
        public AddressUpdated(int id) : base(id)
        {
        }
    }
}
