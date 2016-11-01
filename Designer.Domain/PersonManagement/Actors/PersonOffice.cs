using System;
using Akka.Actor;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Actors;

namespace Designer.Domain.PersonManagement.Actors
{
    /// <summary>
    /// Dispatch person-related tasks to registrator or a person aggregate
    /// </summary>
    public class PersonOffice : OfficeActor<Person, int>
    {
        private IActorRef personRegistrator;

        public PersonOffice(IActorRef eventStore) : base(eventStore)
        {
            personRegistrator = 
                Context.ActorOf(Props.Create<PersonRegistrator>(eventStore), "registrator");

            Receive<RegisterPerson>(r => personRegistrator.Forward(r));
            // all others handled in base class

            // this would help if something fails:
            // Context.Watch(personRegistrator);
            // Receive<Terminated>(t => Console.WriteLine($"Terminated: {t}"));
        }
    }
}
