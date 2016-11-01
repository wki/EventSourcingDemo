using NUnit.Framework;
using System;
using Akka.TestKit;
using Akka.Actor;
using Akka.TestKit.NUnit;
using Designer.Domain.PersonManagement.Actors;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Messages;

namespace Designer.Domain.Tests
{
    [TestFixture]
    public class PersonRegistratorTest : TestKit
    {
        private TestProbe eventStore;
        private IActorRef personRegistrator;

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("eventstore");
            personRegistrator = Sys.ActorOf(Props.Create<PersonRegistrator>(eventStore), "personregistrator");
        }

        [Test]
        public void PersonRegistrator_Initially_ReportedIds()
        {
            // Act
            personRegistrator.Tell(new End());
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<string>("0|1");
        }

        [Test]
        public void PersonRegistrator_AfterRegistration_IncreaseUsableId()
        {
            // Act
            personRegistrator.Tell(new End());
            personRegistrator.Tell(new RegisterPerson("Full", "mail@domain.de"));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<Reply>(r => r.IsOk);
            ExpectMsg<string>("0|2");
        }


        [Test]
        public void PersonRegistrator_AfterMultipleRegistrations_IncreaseUsableId()
        {
            // Act
            personRegistrator.Tell(new End());
            foreach (var mail in new[] { "1@x.de", "2@x.de", "3@x.de" })
                personRegistrator.Tell(new RegisterPerson("Full", mail));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            for (var i=0; i<3; i++)
                ExpectMsg<Reply>(r => r.IsOk);
            ExpectMsg<string>("0|4");
        }

        [Test]
        public void PersonRegistrator_RegisterDuplicatedEmail_IncreaseUsableIdAndRaiseError()
        {
            // Act
            personRegistrator.Tell(new End());
            personRegistrator.Tell(new RegisterPerson("Full", "mail@domain.de"));
            personRegistrator.Tell(new RegisterPerson("Full", "mail@domain.de"));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<Reply>(r => r.IsOk);
            ExpectMsg<Reply>(r => r.Message == "Email 'mail@domain.de' already used");
            ExpectMsg<string>("0|2");
        }

        [Test]
        public void PersonRegistrator_AfterRecover_IncreaseUsableIds()
        {
            // Act
            personRegistrator.Tell(new End());
            personRegistrator.Tell(new PersonRegistered(23, "Full", "mail@domain.de"));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<string>("23|24");
        }
    }
}
