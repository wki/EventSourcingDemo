using System;
using Akka.Actor;
using Designer.Domain.HangtagCreation.Actors;
using Designer.Domain.PersonManagement.Actors;
using Designer.Domain.Rendering.Actors;
using Designer.Domain.Todos.Actors;

namespace Designer.Domain
{
    /// <summary>
    /// using this app service we access all the internals and the actor model
    /// </summary>
    public class DesignerService
    {
        private readonly ActorSystem actorSystem;

        #pragma warning disable 0414
        private readonly IActorRef personAggregateOffice;
        private readonly IActorRef personList;

        private readonly IActorRef hangtagAggregateOffice;
        private readonly IActorRef hangtagDetailOffice;
        private readonly IActorRef hangtagList;

        private readonly IActorRef renderObserverOffice; // process manager

        private readonly IActorRef todoListOffice;
        #pragma warning restore 0414

        public DesignerService()
        {
            actorSystem = ActorSystem.Create("Designer");

            personAggregateOffice = actorSystem.ActorOf(Props.Create<PersonOffice>(), "person");
            personList = actorSystem.ActorOf(Props.Create<PersonList>(), "person-list");

            hangtagAggregateOffice = actorSystem.ActorOf(Props.Create<HangtagOffice>(), "hangtag");
            hangtagDetailOffice = actorSystem.ActorOf(Props.Create<HangtagDetailOffice>(), "hangtag-detail");
            hangtagList = actorSystem.ActorOf(Props.Create<HangtagList>(), "hangtag-list");

            renderObserverOffice = actorSystem.ActorOf(Props.Create<RenderObserver>(), "render-observer");

            todoListOffice = actorSystem.ActorOf(Props.Create<TodoListOffice>(), "todos");
        }
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
