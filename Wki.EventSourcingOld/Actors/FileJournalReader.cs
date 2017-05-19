using System.Collections.Generic;
using System.IO;
using System.Linq;
using Akka.Actor;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Serialisation;
using Wki.EventSourcing.Protocol.EventStore;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Load all events saved in the journal on an initial demand, loads in junks
    /// </summary>
    /// <description>
    /// all loaded events reside inside EventStore.
    /// loading of persisted events happens once at start time of the EventStore 
    /// </description>
    public class FileJournalReader : FileJournalBase
    {
        // allow access of all events in an easy to enumerate way
        private IEnumerator<Event> events;

        public FileJournalReader(string storageDir) : base(storageDir)
        {
            events = AllEvents().GetEnumerator();

            Receive<LoadNextEvents>(l => LoadJournal(l));
        }

        // load a junk of requested size of events
        private void LoadJournal(LoadNextEvents loadJournal)
        {
            var nrEvents = loadJournal.NrEvents;
            var lastReached = true;
            Context.System.Log.Debug("Load {0} Events", nrEvents);

            while (nrEvents-- > 0 && !(lastReached = !events.MoveNext()))
                Sender.Tell(new EventLoaded(events.Current));

            if (lastReached)
            {
                Context.System.Log.Debug("Reached end after {0} Events, End Loading", loadJournal.NrEvents-nrEvents-1);

                Sender.Tell(new EndOfTransmission());
            }
        }

        // Enumerate over event directories and files
        // hopefully being more efficient than a loop only
        private IEnumerable<Event> AllEvents()
        {
            foreach (var year in DirList(storageDir))
                foreach (var month in DirList(year))
                    foreach (var day in FileList(month))
                        foreach (var json in File.ReadAllLines(day))
                            yield return EventSerializer.FromJson(json);
        }

        // helper: return an ordered list of directories inside directory
        private IEnumerable<string> DirList(string directory)
        {
            return Directory
                .EnumerateDirectories(directory)
                .OrderBy(d => Path.GetFileName(d));
        }

        // helper: return an ordered list of files inside directory
        private IEnumerable<string> FileList(string directory)
        {
            return Directory
                .EnumerateFiles(directory)
                .OrderBy(d => Path.GetFileName(d));
        }
    }
}
