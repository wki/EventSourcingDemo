using System;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonRegistrator : DurableActor<int>
    {
        // after persisting this id is updated
        private int lastPersistedId;

        // id to be used for next registration to avoid race conditions
        private int nextUsableId;

        public PersonRegistrator(IActorRef eventStore, int id) : base(eventStore, id)
        {
            lastPersistedId = 0;
            nextUsableId = 1;

            Receive<RegisterPerson>(r => RegisterPerson(r));
            Recover<PersonRegistered>(p => PersonRegistered(p));
        }

        private void RegisterPerson(RegisterPerson registerPerson)
        {
            // TODO: can we check for a duplicate registration?
            //       maybe keep a list of already-known email addresses
            Persist(new PersonRegistered(nextUsableId++));
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            lastPersistedId = personRegistered.Id;

            if (nextUsableId < lastPersistedId +1)
                nextUsableId = lastPersistedId + 1;
        }
    }
}
