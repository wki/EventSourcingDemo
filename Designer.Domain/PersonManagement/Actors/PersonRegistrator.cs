using System;
using System.Collections;
using System.Collections.Generic;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonRegistrator : DurableActor<int>
    {
        private class EmailComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => x.ToLower() == y.ToLower();
            public int GetHashCode(string obj) => obj.ToLower().GetHashCode();
        }

        // keep all emails unique
        private HashSet<string> emailAddresses;

        // after persisting this id is updated
        private int lastPersistedId;

        // id to be used for next registration to avoid race conditions
        private int nextUsableId;

        public PersonRegistrator(IActorRef eventStore, int id) : base(eventStore, id)
        {
            emailAddresses = new HashSet<string>(new EmailComparer());
            lastPersistedId = 0;
            nextUsableId = 1;

            Receive<RegisterPerson>(r => RegisterPerson(r));
            Recover<PersonRegistered>(p => PersonRegistered(p));
        }

        private void RegisterPerson(RegisterPerson registerPerson)
        {
            if (emailAddresses.Contains(registerPerson.Email))
                throw new ArgumentException($"Email '{registerPerson.Email}' already used");

            Persist(new PersonRegistered(nextUsableId++, registerPerson.Fullname, registerPerson.Email));
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            lastPersistedId = personRegistered.Id;
            emailAddresses.Add(personRegistered.Email);

            if (nextUsableId < lastPersistedId + 1)
                nextUsableId = lastPersistedId + 1;
        }
    }
}
