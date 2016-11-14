using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Akka.Actor;
using Designer.Domain.HangtagCreation.Actors;
using Designer.Domain.PersonManagement.Actors;
using Designer.Domain.PersonManagement.DTOs;
using Designer.Domain.PersonManagement.Messages;
using Designer.Domain.Rendering.Actors;
using Designer.Domain.Todos.Actors;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;

namespace Designer.Domain
{
    /// <summary>
    /// using this app service we access all the internals and the actor model
    /// </summary>
    public class DesignerService
    {
        private readonly ActorSystem actorSystem;

#pragma warning disable 0414
        private readonly IActorRef journalReader, journalWriter;
        private readonly IActorRef eventStore;
        private readonly IActorRef personAggregateOffice;
        private readonly IActorRef personList;

        private readonly IActorRef hangtagAggregateOffice;
        private readonly IActorRef hangtagDetailOffice;
        private readonly IActorRef hangtagList;

        private readonly IActorRef renderObserverOffice; // process manager

        private readonly IActorRef todoListOffice;
        #pragma warning restore 0414

        public DesignerService(string storageDir)
        {
            actorSystem = ActorSystem.Create("Designer");

            //var storageDir = ConfigurationManager.AppSettings["storageDir"];
            //if (String.IsNullOrWhiteSpace(storageDir))
            //    throw new NullReferenceException("storageDir is missing or empty");
            //if (!Directory.Exists(storageDir))
            //    Directory.CreateDirectory(storageDir);

            // readers and writers for the EventStore
            journalReader = actorSystem.ActorOf(Props.Create<FileJournalReader>(storageDir), "journal-reader");
            journalWriter = actorSystem.ActorOf(Props.Create<FileJournalWriter>(storageDir), "journal-writer");

            // every actor must know the event store
            eventStore = actorSystem.ActorOf(Props.Create<EventStore>(storageDir, journalReader, journalWriter), "eventstore");

            // build actors
            personAggregateOffice = actorSystem.ActorOf(Props.Create<PersonOffice>(eventStore), "person");
            personList = actorSystem.ActorOf(Props.Create<PersonList>(eventStore), "person-list");

            hangtagAggregateOffice = actorSystem.ActorOf(Props.Create<HangtagOffice>(eventStore), "hangtag");
            hangtagDetailOffice = actorSystem.ActorOf(Props.Create<HangtagDetailOffice>(eventStore), "hangtag-detail");
            hangtagList = actorSystem.ActorOf(Props.Create<HangtagList>(eventStore), "hangtag-list");

            renderObserverOffice = actorSystem.ActorOf(Props.Create<RenderObserver>(eventStore), "render-observer");

            todoListOffice = actorSystem.ActorOf(Props.Create<TodoListOffice>(eventStore), "todos");
        }

        #region Diagnostics
        public Task<StatusReport> GetStatusReport()
        {
            return eventStore.Ask<StatusReport>(new GetStatusReport());
        }
        #endregion

        #region Person Management Use Cases
        public void RegisterPerson(string fullname, string email)
        {
            // evtl. Ask -- damit synchron.
            personAggregateOffice.Tell(new RegisterPerson(fullname, email));
        }

        public Task<IEnumerable<PersonInfo>> ListPersons()
        {
            return personList.Ask<IEnumerable<PersonInfo>>(new ListPersons());
        }

        public Task<PersonInfo> GetPersonState(int id)
        {
            return personAggregateOffice.Ask<PersonInfo>(new GetPersonInfo(id));
        }
        #endregion
    }
}

/*

Hierarchie der Aktoren für unseren "Hangtag Designer"

Bereiche:
  * PersonManagement
  * HangtagCreation
  * Todos
  * Rendering

    User
     |
     +-- hangtag              Office für Hangtag Aggregte Root
     |    |
     |    +-- creator         Anlage neuer oder Duplizierung (fortlaufende Nummer)
     |    |
     |    +-- 1..n            Aggregate root (ID) // Evtl. aufteilen nach Art der Arbeit
     |
     +-- hangtag-detail       Office für Hangtag-View
     |    |
     |    +-- 1..n            Ein View (ID), abhängig von person-list
     |
     +-- hangtag-list         Liste aller Hangtags für Suche (View)
     |
     +-- render-observer      Process-Manager überwacht PDF Erzeugung
     |    |
     |    +-- 1..n            evtl: Kind-Aktor, wird angelegt, wenn warten auf PDF notwendig
     |
     +-- person               Office für Personen Aggregate Root
     |    |
     |    +-- registrator     Registrierung neuer Personen (fortlaufende Nummer)
     |    |
     |    +-- 1..n            Personen Aggregate root (Detail-Info, evtl. abfragbar ?) 
     |
     +-- person-list          Liste aller Personen für Dropdown listen (View)
     |
     +-- todos                Office mit ToDos für einzelne Person (Dependency: hangtag-list)
     |    |
     |    +-- 1..n            View: ToDo für Person ID, lauscht auf person-id Änderungen, subscribe hangtag-list changes
 */
