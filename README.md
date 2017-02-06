# EventSourcing Demo

## Aktor-Typen

1. Office

   Ein Office ist der "Ansprechpartner" für Aktionen, die einen bestimmten
   Typ von Aktor betreffen. Typischerweise leitet er eine eintreffende Nachricht
   an einen Aktor eines bestimmten Typs weiter. Der Ziel-Aktor kann vom Typ oder
   dem Inhalt der Nachricht (z.B. PersistenceId) abhängen.

2. Process Manager

   Ein Process Manager liest seit Systemstart bestimmte Nachrichten mit und bekommt
   so ein "Bild" vom Zusatand bestimmter Teile des Systems. Abhängig vom Zustand
   in Verbindung mit evtl. Zeit kann der Process Manager bestimmte Vorgänge
   (evtl. wiederholt) anstoßen.

   Typisches Problem:
     - während Rekonstruktion darf nix passieren
     - nach Rekonstruktion müssen wir warten bis wir reagieren
     - im Live Zustand können wir sofort reagieren
   
   Ausreichend?
     - mit diesem Ansatz muss 1 Process Manager alle laufenden Dinge beobachten
     - evtl. wäre ein Office-ähnliches Dispatching sinnvoll
     - doch: wie restaurieren ohne Probleme? wie vermeiden von zig Kind-Aktoren?

3. Aggregate Root

   Pro Aggregate Root existiert ein Aktor, der durch ein Office angelegt und seine
   Kommandos vom Office erhält. Ist ein Aggregate root für bestimmte Dauer inaktiv,
   so wird es aus dem Speicher entfernt.

4. View

   Wahlweise hinter einem Office oder direkt kann ein View durch Empfangen bestimmter
   Nachrichten (mit oder ohne Rücksichtnahme auf eine PersistenceId) den Zustand
   für ein oder mehr Aggregate Root wiedergeben. Sitzt ein View hinter einem Office,
   wird eine Instanz für eine PersistenceId bei der ersten Anfrage erstellt und
   bleibt dann bis zu einer bestimmten Inaktivität im Speicher und wird aktualisiert.


## Aktor-Basisklassen

1. DurableActor, DurableActor&lt;TIndex&gt;

   Unterschied: generische Version besitzt eine Id und lauscht nur auf 
   Kommandos vom Typ ```DispatchableCommand<TIndex>``` 

   * Receive&lt;TCommand&gt;(handler)

     DSL innerhalb Konstruktor. Legt Verhalten bei Kommandos fest.

   * Recover&lt;TEvent&gt;(handler)

     DSL innerhalb Konstruktor. Legt Verhalten bei Ereignissen fest.

   * Persist(event)

     Teil der Kommando-Behandlung. Sorgt für Persistierung des angegebenen 
     Ereignisses und ruft nach erfolgreicher Persistierung alle 
     Behandlungsroutinen für dieses Ereignis auf.

2. OfficeActor&lt;TActor, TIndex&gt;

   erlaubt das eigene Behandeln beliebiger Nachrichten. Werden Nachrichten
   nicht behandelt und handelt es sich um ```DispatchableCommand``` 
   Nachrichten, so werden diese an einen notfalls erzeugten Aktor vom Typ
   ```TActor``` gesandt.

3. EventStore

   kümmert sich um alle Persistierungs-Belange.


**Aktoren**

Typ                 | Cmd | Evt | Actor                               | Konstruktor
--------------------|:---:|:---:|-------------------------------------|--------------- 
**Office**          | X   | -   | OfficeActor                         | eventStore
**Process Manager** | -   | X   | DurableActor                        | eventStore
**Aggregate Root**  | X   | X   | DurableActor&lt;T&gt;               | eventStore, id
**View**            | -   | X   | DurableActor / DurableActor&lt;T&gt;| s.o.

**Nachrichten**

Typ         | Basisklasse
------------|-----------------------------------
**Command** | DispatchableCommand&lt;TIndex&gt;
**Query**   | DispatchableCommand&lt;TIndex&gt;
**Event**   | Event / Event&lt;TIndex&gt;
**Document**| -


## Besondere Klassen

1. GetState

   fordert einen Aktor (EventStore, OfficeActor, DurableActor) auf, seinen
   internen Zustand in Form eines immutable XxxState Objektes zu melden.

2. DispatchableCommand&lt;TIndex&gt;

   Basisklasse für Kommandos. Abhängig vom Aktor-Typ evtl. generisch (dann
   mit Index).

3. Event

   Basisklasse für alle Events, enthält eine ```OccuredOn``` Eigenschaft.

4. Reply

   gedacht als Antwort auf Kommandos, um evtl. Validierungsfehler in Form 
   einer Nachricht zu transportieren.

   ```return Reploy.Ok(); // bzw. Reply.Error("message");```

5. EventStore

   muss existieren, wird diversen Aktoren als Konstruktor-Argument mit gegeben


## diverse Protokolle
| Abk.| Bedeutung     |
|-----|---------------|
| ES  | EventStore    |
| DA  | DurableActor  |
| OA  | OfficeActor   |
| JR  | JournalReader |
| JW  | JournalWriter |


 * Zwischen EventStore und JournalReader zum Befüllen EventStore

   - solange bis alle Events geladen: Bearbeitung von Blöcken zu n Ereignissen
     - ES -> JR: LoadJournal(n)
     - JR -> ES: n x EventLoaded(event)
   - wenn fertig:
     - JR -> ES: End


 * Zwischen EventStore und DurableActor zum Restaurieren DurableActor

   - Vorbereitung
     - DA -> ES: StartRestore(interestingEvents)
   - solange bis alle Events geladen: Restaurieren von Blöcken zu n Ereignissen
     - DA -> ES: ResotreEvents(n) 
     - ES -> DA: n x Event
   - wenn fertig
     - ES -> DA: End


 * Weiterleitung von DispatchableCommand abgeleiteten Nachrichten im OfficeActor

   - Eintreffen einer Nachricht
     - X -> OA: command
   - Sicherstellen, dass Durable Actor angelegt ist
     - OA -> DA: command (via Forward)


 * Abfrage Status (DurableActor, OfficeActor, EventStore)

   - Status Abfrage
     - X -> Actor: GetState
     - Actor -> X: DurableActorState | OfficeActorState | EventStoreState


 * Vorgang des Persistierens

   - Persistierung
     - DA -> ES: PersistEvent(event)
     - ES -> JW: PersistEvent(event)
     - JW -> ES: EventPersisted(event)
   - Benachrichtigung aller Subscriber (incl. Absender)
     - ES -> (Subscribers): event


 * Benachrichtigung an EventStore zur Pflege der Subscriber-Liste

   - solange am Leben, regelmäßig
     - DA -> ES: StillAlive
   - unmittelbar vor Lebensende
     - DA -> ES: NotAlive

TODO: wie erhält der Supervisor eine Nachricht vom Ableben?

TODO: kann es Racing Conditions geben wenn ein Kind beim Office stirbt?
