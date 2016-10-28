using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Serialisation;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Load all events saved in the journal on an initial demand, loads in junks
    /// </summary>
    public class FileJournalReader : FileJournalBase
    {
        IEnumerator<Event> events;

        public FileJournalReader(string storageDir) : base(storageDir)
        {
            events = AllEvents().GetEnumerator();

            Receive<LoadJournal>(l => LoadJournal(l));
        }

        // this event may only occur once during this actor's lifecycle
        private void LoadJournal(LoadJournal loadJournal)
        {
            var nrEvents = loadJournal.NrEvents;
            var lastReached = true;
            Context.System.Log.Debug("Load {0} Events", nrEvents);

            while (nrEvents-- > 0 && !(lastReached = !events.MoveNext()))
                Context.Parent.Tell(new EventLoaded(events.Current));

            if (lastReached)
            {
                Context.System.Log.Debug("Reached end after {0} Events, End Loading", loadJournal.NrEvents-nrEvents-1);

                Context.Parent.Tell(new End());
            }
        }

        // Enumerator over event directories and files
        // hopefully being more efficient than a loop only
        private IEnumerable<Event> AllEvents()
        {
            foreach (var year in DirList(StorageDir))
                foreach (var month in DirList(year))
                    foreach (var day in FileList(month))
                        foreach (var json in File.ReadAllLines(day))
                            yield return EventSerializer.FromJson(json);
        }

        private IEnumerable<string> DirList(string directory)
        {
            return Directory
                .EnumerateDirectories(directory)
                .OrderBy(d => Path.GetFileName(d));
        }

        private IEnumerable<string> FileList(string directory)
        {
            return Directory
                .EnumerateFiles(directory)
                .OrderBy(d => Path.GetFileName(d));
        }
    }
}
