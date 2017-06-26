using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol.Retrieval;
using Wki.EventSourcing.Protocol.Persistence;

namespace Wki.EventSourcing.Persistence
{
    /// <summary>
    /// Journal Actor handles all (possibly failing) journalling commands via a IJournalStore implementation
    /// </summary>
    public class Journal: UntypedActor
    {
        private IJournalStore JournalStore;

        public Journal(IJournalStore journalStore)
        {
            JournalStore = journalStore;
        }

        protected override void OnReceive(object message)
        {
            switch(message)
            {
                // a Durable wants a snapshot
                case LoadSnapshot loadSnapshot:
                    var loaded = false;
                    try
                    {
                        if (JournalStore.HasSnapshot(loadSnapshot.PersistenceId))
                        {
                            Sender.Tell(JournalStore.LoadSnapshot(loadSnapshot.PersistenceId, loadSnapshot.StateType));
                            loaded = true;
                        }
                        else
                            Context.System.Log.Debug("No snapshot found for '{0}'", loadSnapshot.PersistenceId);
                    }
                    catch (Exception e)
                    {
                        Context.System.Log.Warning("Could not load Snapshot for '{0}': {1}", loadSnapshot.PersistenceId, e.Message);
                    }

                    if (!loaded)
                        Sender.Tell(NoSnapshot.Instance);

                    break;

                // EventStore wants next Events without filtering
                // a Durable wants next filtered Events (unless EventStore caches them all)
                case LoadNextEvents loadNextEvents:
                    // request one event more than we need
                    // if we get all, we do not have EOF
                    var count = 0;
                    foreach (var e in JournalStore.LoadNextEvents(loadNextEvents.EventFilter, loadNextEvents.NrEvents + 1))
                    {
                        if (++count <= loadNextEvents.NrEvents)
                            Sender.Tell(e);
                        else
                            Sender.Tell(End.Instance);
                    }
                    break;

                // a Durable wants to persist an event
                case PersistEvent persistEvent:
                    try
                    {
                        JournalStore.AppendEvent(persistEvent.PersistenceId, persistEvent.Event);
                        var eventRecord = new EventRecord(JournalStore.LastEventId, DateTime.Now, persistEvent.PersistenceId, persistEvent.Event);
                        Sender.Tell(new EventPersisted(eventRecord));
                    }
                    catch(Exception e)
                    {
                        Context.System.Log.Error("Failed persisting Event {0} for {1}: {2}", persistEvent.Event.GetType().Name, persistEvent.PersistenceId, e.Message);
                        Sender.Tell(new PersistEventFailed(persistEvent.PersistenceId, persistEvent.Event, e.Message));
                    }
                    break;

                // a Durable wants to persist a snapshot
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
