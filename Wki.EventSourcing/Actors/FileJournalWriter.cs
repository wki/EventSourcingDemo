using System;
using System.IO;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Serialisation;
using Wki.EventSourcing.Util;

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
            Console.WriteLine($"Persist Event {persistEvent}");
            Context.System.Log.Debug("Persist Event {0}", persistEvent.GetType().Name);

            var @event = persistEvent.Event;
            var now = SystemTime.Now;
            var file = Path.Combine(Dir(now), $"{now:dd}.json");
            Console.WriteLine($"File: {file}");

            File.AppendAllLines(file, new[] { EventSerializer.ToJson(@event) });

            Console.WriteLine($"Sender: {Sender}");
            Sender.Tell(new EventPersisted(@event));
        }
    }
}
