# EventSourcing Demo

## Aktor-Typen (aus Prozess-Sicht)

0. EventStore

   Zweck:
     - Ereignisse Persistieren und wieder laden
     - Observer bedienen, Rekonstruktion ausführen

   Verhalten:
     - Schreib- / Lese-Operationen an JournalWriter + JournalReader weiterleiten
     - Rekonstruktion ausführen


1. Office

   Zweck:
     - Absender von Command-Nachrichten sprechen mit einer Stelle
     - Empfänger (View oder Aggregate root) werden notfalls erzeugt und restauriert

   Verhalten:
     - Command-Nachrichten an einen bestimmten Aktor-Typ (der notfalls neu erzeugt wird) weiter abgeleitet.
     - Per eigenem Code können beliebige andere Command-Nachrichten individuell behandelt werden.
     - Unbehandelte Nachrichten loggen und ignorieren


2. Process Manager

   Zweck:
     - bestimmte Ereignisse mitlesen, Zustand bestimmter Dinge mit protokollieren.
     - Abhängig vom Zustand durch Kommandos einfluß nehmen.

   Typisches Problem:
     - während Rekonstruktion darf nix passieren
     - nach Rekonstruktion müssen wir warten bis wir reagieren
     - im Live Zustand können wir sofort reagieren
   
   Ausreichend?
     - mit diesem Ansatz muss 1 Process Manager alle laufenden Dinge beobachten
     - evtl. wäre ein Office-ähnliches Dispatching sinnvoll
     - doch: wie restaurieren ohne Probleme? wie vermeiden von zig Kind-Aktoren?


3. Aggregate Root

   Zweck:
     - veränderbaren Zustand für ein individuelles "Ding" speichern
     - Bei Veränderungen Events auslösen
     - restaurierbar über Events
     - nur für bestimmte Dauer aktiv

   Verhalten:
     - während Rekonstruktion Commands zwischenpuffern
     - immer Events verarbeiten und Zustand verändern
     - Nach Konstruktion Commands + Events verarbeiten


4. View

   Zweck:
     - lesbaren Zustand für ein oder mehrere Dinge generieren

   Verhalten:
     - wahlweise PersistenceId berücksichtigen
     - bestimmte Events mitlesen
     - Zustand ableiten


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

4. JournalReader

   liest alle gespeicherten Ereignisse und übergibt sie dem EventStore
   zur Pufferung.

5. JournalWriter

   Erhält ein Ereignis zur Persistierung und meldet die erfolgreiche
   SPeicherung, damit danach erst das Ereignis weiterverarbeiet wird.


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

   ```return Reply.Ok(); // bzw. Reply.Error("message");```

   Idee: ```Reply.Value("some value"); Reply.Value(new Bla());```


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


 * EventStore füllen

   - solange bis alle Events geladen: Bearbeitung von Blöcken zu n Ereignissen
     - ES -> JR: LoadJournal(n)
     - JR -> ES: n x EventLoaded(event)
   - wenn fertig:
     - JR -> ES: End

 * Subscribe / Unsubscribe

   - DA bei PreStart -> ES: Subscribe(InterestingEvents)
   - DA bei PostStop -> ES: Unsubscribe

 * Durable Actor laden

   - Voraussetzung: Subscribe ist erfolgt
   - Vorbereitung
     - DA -> ES: StartRestore
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

 * Alive / Graceful Passivation

   - OA -> DA: erzeugen
   - DA -> OA: StillAlive
   - DA bei Inaktivität -> OA: Passivate
   - OA -> DA: terminieren

 * div. Abfragen

   - Statistik Abfrage
     - X -> Actor: GetStatistics
     - Actor -> X: DurableActorStatistics | OfficeActorStatistics | EventStoreStatistics

 * Vorgang des Persistierens

   - Persistierung
     - DA -> ES: PersistEvent(event)
     - ES -> JW: PersistEvent(event)
     - JW -> ES: EventPersisted(event)
   - Benachrichtigung aller Subscriber (incl. Absender)
     - ES -> (Subscribers): event
