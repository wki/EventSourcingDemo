using System;
using Designer.Domain.HangtagCreation.Messages;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagDetail : DurableActor<int>
    {
        public HangtagDetail(int id) : base(id)
        {
            // person-list subscriben auf änderungen
            Context.System.EventStream.Subscribe(Self, typeof(PersonListUpdated));
            Receive<PersonListUpdated>(p => PersonListUpdated(p));

            // Alles interessante beobachten
            Recover<HangtagCreated>(h => HangtagCreated(h));
            Recover<HangtagCloned>(h => HangtagCloned(h));
        }

        private void HangtagCreated(HangtagCreated hangtagCreated)
        {
        }

        private void HangtagCloned(HangtagCloned hangtagCloned)
        {
        }

        private void PersonListUpdated(PersonListUpdated personListUpdated)
        {
            // fordere Personenliste an.
        }
    }
}
