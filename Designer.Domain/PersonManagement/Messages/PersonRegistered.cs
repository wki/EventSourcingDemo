using System;
using Wki.EventSourcing.Protocol;

namespace Designer.Domain.PersonManagement.Messages
{
    public class PersonRegistered : Event<int>
    {
        public string Fullname { get; private set; }
        public string Email { get; private set; }

        public PersonRegistered(int id, string fullname, string email) : base(id)
        {
            Fullname = fullname;
            Email = email;
        }
    }
}
