using System;
using System.Collections.Generic;
using Akka.Actor;
using Designer.Domain.PersonManagement.DTOs;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonList : DurableActor
    {
        private List<PersonInfo> Persons;

        public PersonList(IActorRef eventStore) : base(eventStore)
        {
            Persons = new List<PersonInfo>();

            Recover<PersonRegistered>(p => PersonRegistered(p));
            Recover<LanguageAdded>(l => LanguageAdded(l));
            Recover<LanguageRemoved>(l => LanguageRemoved(l));
            Recover<AddressUpdated>(a => AddressUpdated(a));
        }

        private void PersonRegistered(PersonRegistered personRegistered)
        {
            // TODO: fill
            Persons.Add(new PersonInfo());

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
