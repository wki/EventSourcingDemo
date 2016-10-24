using System;
using Akka.Actor;

namespace Designer.Domain
{
    /// <summary>
    /// using this app service we access all the internals and the actor model
    /// </summary>
    public class DesignerService
    {
        private readonly ActorSystem actorSystem;

        public DesignerService()
        {
            actorSystem = ActorSystem.Create("Designer");

            // bestimmte Aktoren anlegen...
        }
    }
}

/*

Hierarchie der Aktoren für unseren "Hangtag Designer"

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