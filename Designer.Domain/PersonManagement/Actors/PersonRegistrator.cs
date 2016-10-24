using System;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonRegistrator : DurableActor<Person>
    {
        public PersonRegistrator()
        {
            Command<RegisterPerson>(r => RegisterPerson(r));
        }
    }
}
