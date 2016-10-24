using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    public class PersonRegistered : Event<int>
    {
        public PersonRegistered(int id) : base(id)
        {
        }
    }
}
