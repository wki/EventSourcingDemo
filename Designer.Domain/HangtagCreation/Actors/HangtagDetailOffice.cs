using System;
using Akka.Actor;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagDetailOffice : OfficeActor<HangtagDetail, int>
    {
        public HangtagDetailOffice(IActorRef eventStore) : base(eventStore)
        {
        }
    }
}
