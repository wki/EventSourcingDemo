using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagList : DurableActor
    {
        public HangtagList(IActorRef eventStore) : base(eventStore)
        {
            // Id is null and we observe messages for every Id
        }
    }
}
