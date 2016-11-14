using System;
using Akka.Actor;
using Designer.Domain.PersonManagement.DTOs;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    /// <summary>
    /// Person aggregate
    /// </summary>
    public class Person : DurableActor<int>
    {
        public string Fullname { get; set; }
        public string Email { get; set; }

        public Person(IActorRef eventStore, int id) : base(eventStore, id)
        {
            Receive<AddLanguage>(l => AddLanguage(l));
            Receive<RemoveLanguage>(l => RemoveLanguage(l));
            Receive<UpdateAddress>(a => UpdateAddress(a));
            Receive<GetPersonInfo>(_ => GetPersonInfo());

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

        private void GetPersonInfo()
        {
            Sender.Tell(new PersonInfo(Id, Fullname, Email));
        }
        #endregion

        #region event handlers
        private void PersonRegistered(PersonRegistered personRegistered)
        {
            Fullname = personRegistered.Fullname;
            Email = personRegistered.Email;
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
