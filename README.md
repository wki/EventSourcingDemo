# EventSourcing Demo

## Aktor-Typen (aus Prozess-Sicht)

0. EventStore

   Zweck:
     - Ereignisse Persistieren und wieder laden
     - Observer bedienen, Rekonstruktion ausführen

   Verhalten:
     - Schreib- / Lese-Operationen an Journal weiterleiten
     - Rekonstruktion ausführen
  
   Anlage:
     - mit Referenz auf Journal, damit Implementierung gewählt


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
     - regelmäßige Snapshots vermeiden zig Kind-Aktoren
     - pro echtem Workflow gibt es dann noch einen eigenen DurableActor<...>


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
     - Zustand losgelöst vom Aktor (State/IState)


4. View

   Zweck:
     - lesbaren Zustand für ein oder mehrere Dinge generieren

   Verhalten:
     - wahlweise PersistenceId berücksichtigen
     - bestimmte Events mitlesen
     - Zustand ableiten
     - bei Änderungen am Zustand: mitteilen damit Clients sich aktualisieren können


## Namensräume

 * Wki.EventSourcing -- Aktor Basisklassen, Interfaces etc.
 * Wki.EventSourcing.Persistence -- Gemeinsamkeiten
 * Wki.EventSourcing.Persistence.Ef -- Entity Framework Core
 * Wki.EventSourcing.Persistence.Npgsql -- NpgSql
 * Wki.EventSourcing.Protocol -- interne Nachrichten
 * Wki.EventSourcing.Statistics -- Statistik-Klassen
 * Wki.EventSourcing.Util -- Konstanten, Serialisierer etc.


## Diverse (Basis-)Klassen

 * ```IEvent```, ```IEvent<TIndex>``` -- Event Interface
 
 * ```ICommand```, ```ICommand<TIndex>``` -- Command Interface

 * ```EventFilter```, ```WantEvents``` -- Event Filter DSL

 * ```EventRecord``` -- persistierte Form eines Events

 * ```State<TState>``` FIXME: besser ```IState<TState>```? 

 * ```SubscribingActor``` -- Basisklasse für DurableActor // notwendig?

 * ```OfficeActor<TClerk, TIndex>``` -- Erzeugung und Verteilung an Clerks

   Für Aggregate, View Offices

 * ```DurableActor```, ```DurableActor<TIndex>```
   
   Für Aggregate Clerks, Views oder View Clerks und ProcessManager


1. ```DurableActor```, ```DurableActor<TIndex, TState>```

   Unterschied: generische Version besitzt eine Id und lauscht nur auf 
   Kommandos vom Typ ```DispatchableCommand<TIndex>``` 

   * Persist(IEvent)

     Teil der Kommando-Behandlung. Sorgt für Persistierung des angegebenen 
     Ereignisses und ruft nach erfolgreicher Persistierung Apply() auf.
  
   * CreateSnapshot() // nur ```DurableActor<TIndex, TState>```

     erstellt einen Snapshot des aktuellen Zustandes. Kann fehl schlagen, wird aber dann ignoriert.
  
   * Apply(IEvent)

     muss im Aktor überladen werden, damit Event auf den Aktor angewandt wird.

   * BuildInitialState() // nur ```DurableActor<TIndex, TState>```

    muss im Aktor überladen werden.

2. ```OfficeActor<TClerk, TIndex>```

   erlaubt das eigene Behandeln beliebiger Nachrichten. Werden Nachrichten
   nicht behandelt und handelt es sich um ```DispatchableCommand``` 
   Nachrichten, so werden diese an einen notfalls erzeugten Aktor vom Typ
   ```TClerk``` gesandt.

3. ```EventStore```

   kümmert sich um alle Persistierungs-Belange.

4. ```Journal```

   liest alle gespeicherten Ereignisse und übergibt sie dem EventStore
   zur Pufferung.
   Erhält ein Ereignis zur Persistierung und meldet die erfolgreiche
   Speicherung, damit danach erst das Ereignis weiterverarbeiet wird.


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

| Abk.| Bedeutung     | Protokoll(e)
|-----|---------------|-----------------------
| ES  | EventStore    | load, subscribe, reconstitute, persist
| DA  | DurableActor  | subscribe, reconstitute, livecycle, persist
| OA  | OfficeActor   | forward, livecycle
| J   | Journal       | load, persist


 * load (Journal)

   - solange bis alle Events geladen: Bearbeitung von Blöcken zu n Ereignissen
     - ES -> J: LoadNextEvents(n)
     - J -> ES: evtl. OfferSnapshot falls start Index = -1 und Snapshot vorhanden
   - wenn fertig:
     - J -> ES: End

 * subscribe

   - DA bei PreStart -> ES: Subscribe(InterestingEvents)
   - DA bei PostStop -> ES: Unsubscribe

 * reconstitute (Durable Actor laden)

   - Voraussetzung: Subscribe ist erfolgt
   - Vorbereitung
     - DA -> ES: StartRestore
   - Snapshot anfordern (falls vorhanden)
     - ES -> J: LoadSnapshot
     - J -> ES: OfferSnapshot / NoSnapshot
     - ES -> DA: OfferSnapshot
   - solange bis alle Events geladen: Restaurieren von Blöcken zu n Ereignissen
     - DA -> ES: RestoreEvents(n) 
     - ES -> DA: n x Event
   - wenn fertig
     - ES -> DA: End

 * forward (Weiterleitung von DispatchableCommand abgeleiteten Nachrichten im OfficeActor)

   - Eintreffen einer Nachricht
     - X -> OA: command
   - Sicherstellen, dass Durable Actor angelegt ist
     - OA -> DA: command (via Forward)

 * livecycle (Alive / Graceful Passivation)

   - OA -> DA: erzeugen
   - DA -> OA: StillAlive
   - DA bei Inaktivität -> OA: Passivate
   - OA -> DA: terminieren

 * info

   - Statistik Abfrage
     - X -> Actor: GetStatistics
     - Actor -> X: DurableActorStatistics | OfficeActorStatistics | EventStoreStatistics

 * persist

   - wahlweise: Persistierung Snapshot
     - DA -> ES: PersistSnapshot(state)
     - ES -> J: PersistSnapshot(state)

   - Persistierung Event
     - DA -> ES: PersistEvent(event)
     - ES -> J: PersistEvent(event)
     - J -> ES: EventPersisted(event)
   - Benachrichtigung aller Subscriber (incl. Absender)
     - ES -> (Subscribers): event
