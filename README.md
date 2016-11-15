# EventSourcing Demo

## Aktoren

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
------------|---
**Command** | DispatchableCommand / DispatchableCommand&lt;TIndex&gt;
**Query**   | DispatchableCommand / DispatchableCommand&lt;TIndex&gt;
**Event**   | Event
**Document**| -


## Besondere Klassen

1. GetState

   fordert einen Aktor (EventStore, OfficeActor, DurableActor) auf, seinen
   internen Zustand in Form eines immutable XxxState Objektes zu melden.

2. DispatchableCommand, DispatchableCommand&lt;TIndex&gt;

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

