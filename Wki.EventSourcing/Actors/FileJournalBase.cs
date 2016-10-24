using System;
using System.IO;
using Akka.Actor;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Base class for File Journal Writer and Reader
    /// </summary>
    /// <description>
    /// The journal is organized as a directory tree
    ///    year / month / day.json
    /// Every file consists of one json-object per line
    /// </description>
    public class FileJournalBase : ReceiveActor
    {
        protected string StorageDir;

        public FileJournalBase(string storageDir)
        {
            StorageDir = storageDir;
        }

        protected string Dir(DateTime day)
        {
            var dir = Path.Combine(StorageDir, String.Format("{0:yyyy}", day), String.Format("{0}:MM}", day));
            if (!Directory.Exists(dir))
            {
                Context.System.Log.Info("Creating storage Dir for month {0}", dir);
                Directory.CreateDirectory(dir);
            }

            return dir;
        }
    }
}
