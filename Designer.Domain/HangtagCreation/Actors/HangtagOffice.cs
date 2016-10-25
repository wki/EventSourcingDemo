using System;
using Akka.Actor;
using Designer.Domain.HangtagCreation.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.HangtagCreation.Actors
{
    public class HangtagOffice : OfficeActor<Hangtag, int>
    {
        private IActorRef hangtagCreator;

        public HangtagOffice()
        {
            hangtagCreator =
                Context.ActorOf(Props.Create<HangtagCreator>(), "creator");

            Receive<CreateHangtag>(c => hangtagCreator.Forward(c));
            Receive<CloneHangtag>(c => hangtagCreator.Forward(c));
        }
    }
}
