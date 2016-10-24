using System;
using Akka.Actor;

namespace Wki.EventSourcing.Actors
{
    /// <summary>
    /// Base class for journal writer and journal reader
    /// </summary>
    /// <description>
    /// Thr journal is written as JSON:
    ///  - one line of JSON per event
    ///  - 1000 lines in a file
    ///  - file names start with the date they have been created
    ///    e.g. 20161019_101342_events.json
    ///  - the Writer searches for the last file and decides if append is possible
    ///  - the Reader sequentially reads all files
    /// </description>
    public class JournalActor : ReceiveActor
    {
    }
}
