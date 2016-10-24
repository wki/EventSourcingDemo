using System;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Messages
{
    // not of base class 'Event', not persisted. Published via EventStream
    public class PersonListUpdated
    {
        public PersonListUpdated()
        {
        }
    }
}
