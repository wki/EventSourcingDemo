using Akka.Actor;
using Akka.TestKit.NUnit;
using Designer.Domain.PersonManagement.Actors;
using NUnit.Framework;
using Wki.EventSourcing.Protocol.Statistics;

namespace Designer.Domain.Tests
{
    [TestFixture]
    public class PersonOfficeTest : TestKit
    {
        [Test]
        public void PersonOffice_Initiating_Works()
        {
            // Arrange
            var eventStore = CreateTestProbe("eventStore");
            var personOffice = Sys.ActorOf(Props.Create<PersonOffice>(eventStore), "person-office");

            // Act
            personOffice.Tell(new GetSize());

            // Assert
            ExpectMsg<int>(0);
        }
    }
}
