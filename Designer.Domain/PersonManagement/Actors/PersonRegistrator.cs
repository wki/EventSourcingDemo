using System;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonRegistrator : DurableActor<Person>
    {
        // the Id for the next person being registered
        private int nextId;

        public PersonRegistrator()
        {
            nextId = 1;

            Command<RegisterPerson>(r => RegisterPerson(r));
            Recover<PersonRegistered>(p => PersonRegistered(p));
        }

        private void RegisterPerson(RegisterPerson registerPerson)
        {
            // TODO: can we check for a duplicate registration?
            //       maybe keep a list of already-known email addresses
            // here we have a race condition when 2 registrations arrive too fast.
            Persist(new PersonRegistered(nextId));
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            nextId = personRegistered.Id + 1;
        }
    }
}
