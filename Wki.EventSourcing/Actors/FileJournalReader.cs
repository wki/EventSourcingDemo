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
    /// Load all events saved in the journal on an initial demand
    /// </summary>
    public class FileJournalReader : FileJournalBase
    {
        public FileJournalReader(string storageDir) : base(storageDir)
        {
            Receive<LoadJournal>(_ => LoadJournal());
        }

        private void LoadJournal()
        {
            var stopwatch = new Stopwatch();
            Context.System.Log.Debug("Load Journal Starting");

            foreach (var year in DirList(StorageDir))
                foreach (var month in DirList(year))
                    foreach (var day in FileList(month))
                        foreach (var json in File.ReadAllLines(day))
                        {
                            var @event = EventSerializer.FromJson(json);
                            Context.Parent.Tell(new EventLoaded(@event));
                        }
        
            Context.System.Log.Info("Load Journal Complete, Duration: {0:F1}s", stopwatch.ElapsedMilliseconds / 1000.0);
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
