using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol.Load;
using Wki.EventSourcing.Protocol.Persistence;

namespace Wki.EventSourcing.Persistence
{
    public class Journal: UntypedActor
    {
        public IJournalStore JournalStore;

        public Journal(IJournalStore journalStore)
        {
            JournalStore = journalStore;
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                case LoadSnapshot loadSnapshot:
                    break;

                case LoadNextEvents loadNextEvents:
                    var filter = WantEvents.StartingAfterEventId(loadNextEvents.StartAfterEventId);
                    foreach (var e in JournalStore.LoadNextEvents(filter))
                        Sender.Tell(e);
                    // wie EOF entscheiden?
                    break;

                case PersistEvent persistEvent:
                    try
                    {
                        JournalStore.AppendEvent(persistEvent.PersistenceId, persistEvent.Event);
                        var eventRecord = new EventRecord(JournalStore.LastEventId, DateTime.Now, persistEvent.PersistenceId, persistEvent.Event);
                        Sender.Tell(new EventPersisted(eventRecord));
                    }
                    catch(Exception e)
                    {
                        Sender.Tell(new PersistEventFailed(persistEvent.PersistenceId, persistEvent.Event, e.Message));
                    }
                    break;

                case PersistSnapshot persistSnapshot:
                    break;
            }
        }
    }
}
