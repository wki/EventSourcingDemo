using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Designer.Domain.PersonManagement.DTOs;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    /// <summary>
    /// Hold a list of all persons known to the system with essential information
    /// </summary>
    public class PersonList : DurableActor
    {
        private Dictionary<int, PersonInfo> Persons;

        public PersonList(IActorRef eventStore) : base(eventStore)
        {
            Persons = new Dictionary<int, PersonInfo>();

            Receive<ListPersons>(_ => Sender.Tell(Persons.Values.ToList()));

            Recover<PersonRegistered>(p => PersonRegistered(p));
            Recover<LanguageAdded>(l => LanguageAdded(l));
            Recover<LanguageRemoved>(l => LanguageRemoved(l));
            Recover<AddressUpdated>(a => AddressUpdated(a));
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            Persons.Add(
                personRegistered.Id, 
                new PersonInfo(personRegistered.Id, personRegistered.Fullname, personRegistered.Email)
            );

            if (!IsRestoring)
                Context.System.EventStream.Publish(new PersonListUpdated());
        }

        private void LanguageAdded(LanguageAdded languageAdded)
        {
            // TODO: update list
        }

        private void LanguageRemoved(LanguageRemoved languageRemoved)
        {
            // TODO: update list
        }

        private void AddressUpdated(AddressUpdated addressUpdated)
        {
            // TODO: update list
        }
    }
}
