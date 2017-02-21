using NUnit.Framework;
using System;
using Akka.TestKit;
using Akka.Actor;
using Akka.TestKit.NUnit;
using Designer.Domain.PersonManagement.Actors;
using Designer.Domain.PersonManagement.Messages;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Protocol.EventStore;

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
        public void PersonRegistrator_Initially_DoesNotAnswer()
        {
            // Act
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void PersonRegistrator_AfterRestore_ReportedIds()
        {
            // Arrange
            personRegistrator.Tell(new End());

            // Act
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<string>("0|1");
        }

        [Test]
        public void PersonRegistrator_AfterRegistration_IncreaseUsableId()
        {
            // Arrange
            personRegistrator.Tell(new End());

            // Act
            personRegistrator.Tell(new RegisterPerson("Full", "mail@domain.de"));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<Reply>(r => r.IsOk);
            ExpectMsg<string>("0|2");
        }


        [Test]
        public void PersonRegistrator_AfterMultipleRegistrations_IncreaseUsableId()
        {
            // Arrange
            personRegistrator.Tell(new End());

            // Act
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
            // Arrange
            personRegistrator.Tell(new End());

            // Act
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
            // Arrange
            personRegistrator.Tell(new End());

            // Act
            personRegistrator.Tell(new PersonRegistered(23, "Full", "mail@domain.de"));
            personRegistrator.Tell(new ReturnIds());

            // Assert
            ExpectMsg<string>("23|24");
        }
    }
}
