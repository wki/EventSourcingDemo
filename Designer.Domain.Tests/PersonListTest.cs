using System;
using System.Collections.Generic;
using System.Linq;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using Designer.Domain.PersonManagement.Actors;
using Designer.Domain.PersonManagement.DTOs;
using Designer.Domain.PersonManagement.Messages;
using NUnit.Framework;
using Wki.EventSourcing.Protocol.EventStore;

namespace Designer.Domain.Tests
{
    [TestFixture]
    public class PersonListTest : TestKit
    {
        private TestProbe eventStore;
        private IActorRef personList;

        [SetUp]
        public void SetUp()
        {
            eventStore = CreateTestProbe("eventstore");
            personList = Sys.ActorOf(Props.Create<PersonList>(eventStore), "personlist");
        }

        [Test]
        public void PersonList_Initially_DoesNotAnswer()
        {
            // Act
            personList.Tell(new ListPersons());

            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void PersonList_AfterRestore_Answers()
        {
            // Arrange
            personList.Tell(new EndOfTransmission());

            // Act
            personList.Tell(new ListPersons());

            // Assert
            ExpectMsg<List<PersonInfo>>(l => l.Count == 0);
        }

        [Test]
        public void PersonList_AfterRegistration_ListsPersons()
        {
            // Arrange
            personList.Tell(new EndOfTransmission());

            // Act
            personList.Tell(new PersonRegistered(1, "f1", "e1@x.de"));
            personList.Tell(new PersonRegistered(13, "f13", "e13@x.de"));
            personList.Tell(new ListPersons());

            // Assert
            ExpectMsg<List<PersonInfo>>(l => String.Join("|", l.Select(i => $"{i.Id},{i.Fullname},{i.Email}")) == "1,f1,e1@x.de|13,f13,e13@x.de");
        }
    }
}
