using System;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class Person : DurableActor<int>
    {
        public Person(IActorRef eventStore, int id) : base(eventStore, id)
        {
            Receive<AddLanguage>(l => AddLanguage(l));
            Receive<RemoveLanguage>(l => RemoveLanguage(l));
            Receive<UpdateAddress>(a => UpdateAddress(a));

            Recover<PersonRegistered>(p => PersonRegistered(p));
            Recover<LanguageAdded>(l => LanguageAdded(l));
            Recover<LanguageRemoved>(l => LanguageRemoved(l));
            Recover<AddressUpdated>(a => AddressUpdated(a));
        }

        #region command handlers
        private void AddLanguage(AddLanguage addLanguage)
        {
        }

        private void RemoveLanguage(RemoveLanguage removeLanguage)
        {
        }

        private void UpdateAddress(UpdateAddress updateAddress)
        {
        }
        #endregion

        #region event handlers
        private void PersonRegistered(PersonRegistered personRegistered)
        {
            // TODO: set all info from registration, ID is alread set via construction
        }

        private void LanguageAdded(LanguageAdded languageAdded)
        {
        }

        private void LanguageRemoved(LanguageRemoved languageRemoved)
        {
        }

        private void AddressUpdated(AddressUpdated addressUpdated)
        {
        }
        #endregion
    }
}
