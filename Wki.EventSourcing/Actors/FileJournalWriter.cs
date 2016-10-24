using System;
using System.IO;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Serialisation;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Write new persisted events to the journal
    /// </summary>
    public class FileJournalWriter : FileJournalBase
    {
        public FileJournalWriter(string storageDir) : base(storageDir)
        {
            Receive<PersistEvent>(p => PersistEvent(p));
        }

        private void PersistEvent(PersistEvent persistEvent)
        {
            Context.System.Log.Debug("Persist Event {0}", persistEvent.GetType().Name);

            var @event = persistEvent.Event;
            var now = DateTime.Now;
            var file = Path.Combine(Dir(now), String.Format("{0:dd}.json", now));

            File.AppendAllText(file, EventSerializer.ToJson(@event));

            Sender.Tell(new EventPersisted(@event));
        }
    }
}
