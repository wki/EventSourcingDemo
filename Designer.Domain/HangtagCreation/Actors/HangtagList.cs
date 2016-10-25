using System;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagList : DurableActor<int?>
    {
        public HangtagList()
        {
            // Id is null and we observe messages for every Id
        }
    }
}
