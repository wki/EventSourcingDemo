using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.PersonManagement.Actors
{
    /// <summary>
    /// Manage Person Registration with unique eMails and sequential IDs
    /// </summary>
    public class PersonRegistrator : DurableActor
    {
        private class EmailComparer : IEqualityComparer<string>
        {
            public bool Equals(string x, string y) => x.ToLower() == y.ToLower();
            public int GetHashCode(string obj) => obj.ToLower().GetHashCode();
        }

        // in order to keep all emails unique we must know them all
        private HashSet<string> emailAddresses;

        // after persisting this id is updated
        private int lastPersistedId;

        // id to be used for next registration to avoid race conditions
        private int nextUsableId;

        public PersonRegistrator(IActorRef eventStore) : base(eventStore)
        {
            emailAddresses = new HashSet<string>(new EmailComparer());
            lastPersistedId = 0;
            nextUsableId = 1;

            Receive<RegisterPerson>(r => RegisterPerson(r));
            Recover<PersonRegistered>(p => PersonRegistered(p));

            // diagnostic messages for testing
            Receive<ListEmailAddresses>(_ => Sender.Tell(String.Join("|", emailAddresses)));
            Receive<ReturnIds>(_ => Sender.Tell($"{lastPersistedId}|{nextUsableId}"));
        }

        private void RegisterPerson(RegisterPerson registerPerson)
        {
            if (emailAddresses.Contains(registerPerson.Email))
            {
                // throw new ArgumentException($"Email '{registerPerson.Email}' already used");
                Sender.Tell(Reply.Error($"Email '{registerPerson.Email}' already used"));
                return;
            }

            emailAddresses.Add(registerPerson.Email);
            Persist(new PersonRegistered(nextUsableId++, registerPerson.Fullname, registerPerson.Email));
            Sender.Tell(Reply.Ok());
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            lastPersistedId = personRegistered.Id;

            if (nextUsableId < lastPersistedId + 1)
                nextUsableId = lastPersistedId + 1;
        }
    }
}
