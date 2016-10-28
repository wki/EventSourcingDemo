using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class Hangtag : DurableActor<int>
    {
        public Hangtag(IActorRef eventStore, int id) : base(eventStore, id)
        {
            // TODO: Use Cases umsetzen
        }
    }
}
