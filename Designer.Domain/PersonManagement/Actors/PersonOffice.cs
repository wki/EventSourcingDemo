using System;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    public class PersonOffice : OfficeActor<Person, int>
    {
        private IActorRef personRegistrator;

        public PersonOffice(IActorRef eventStore) : base(eventStore)
        {
            personRegistrator = 
                Context.ActorOf(Props.Create<PersonRegistrator>(), "registrator");

            Receive<RegisterPerson>(r => personRegistrator.Forward(r));
            // all others handled in base class
        }
    }
}
