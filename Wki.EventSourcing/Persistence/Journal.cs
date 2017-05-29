using System;
using Akka.Actor;
using Wki.EventSourcing.Protocol;

namespace Wki.EventSourcing.Persistence
{
    public class Journal : UntypedActor
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
                case LoadNextEvents loadNextEvents:
                    foreach (var e in JournalStore.LoadNextEvents(WantEvents.StartingAtExcluding(loadNextEvents.FromPosExcluding)))
                        Sender.Tell(e);
                    // wie EOF entscheiden?
                    break;
            }
        }
    }
}
