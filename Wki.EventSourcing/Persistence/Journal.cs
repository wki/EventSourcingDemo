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
                    var loadedSnapshot = false;
                    try
                    {
                        if (JournalStore.HasSnapshot(loadSnapshot.PersistenceId))
                        {
                            Sender.Tell(JournalStore.LoadSnapshot(loadSnapshot.PersistenceId, loadSnapshot.StateType));
                            loadedSnapshot = true;
                        }
                        else
                            Context.System.Log.Debug("No snapshot found for '{0}'", loadSnapshot.PersistenceId);
                    }
                    catch (Exception e)
                    {
                        Context.System.Log.Warning("Could not load Snapshot for '{0}': {1}", loadSnapshot.PersistenceId, e.Message);
                    }

                    if (!loadedSnapshot)
                        Sender.Tell(NoSnapshot.Instance);

                    break;

                case LoadNextEvents loadNextEvents:
                    var filter = WantEvents.StartingAfterEventId(loadNextEvents.StartAfterEventId);
                    foreach (var e in JournalStore.LoadNextEvents(filter))
                        Sender.Tell(e);
                    // FIXME: wie EOF entscheiden? -- NrEvents + 1 laden, wenn noch einer übrig -> kein EOF
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
                    try
                    {
                        JournalStore.SaveSnapshot(persistSnapshot.PersistenceId, persistSnapshot.State, persistSnapshot.LastEventId);
                    }
                    catch(Exception e)
                    {
                        // a failing snapshot save is not fatal. Just log and we are fine.
                        Context.System.Log.Warning("Failed creating a snapshot for {0}: {1}", persistSnapshot.PersistenceId, e.Message);
                    }
                    break;
            }
        }
    }
}
