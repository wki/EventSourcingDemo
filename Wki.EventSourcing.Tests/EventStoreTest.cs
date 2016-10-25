using System;
using NUnit.Framework;
using Akka.TestKit.NUnit;
using System.IO;
using Akka.Configuration;
using Akka.Actor;
using Wki.EventSourcing.Actors;
using System.Threading;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class EventStoreTest : TestKit
    {
        private string storageDir;
        private IActorRef eventStore;

        [SetUp]
        public void SetUp()
        {
            storageDir = CreateTempDir();
            var config = $@"
            eventstore.dir = ""{storageDir}""
            eventstore.writer = ""Wki.EventSourcing.Tests.Actors.Writer""
            eventstore.reader = ""Wki.EventSourcing.Tests.Actors.Reader""
            ";

            Sys.Settings.Config
               .WithFallback(ConfigurationFactory.ParseString(config));

            Console.WriteLine(config);
        
            eventStore = Sys.ActorOf(Props.Create<EventStore>(), "eventstore");
        }

        [TearDown]
        public void TearDown()
        {
            RemoveTempDir(storageDir);
        }

        [Test]
        public void UnitOfWork_StateUnderTest_ExpectedBehaviour()
        {
            // Arrange

            // Act

            // Assert
            // Thread.Sleep(TimeSpan.FromSeconds(30));
            Assert.IsTrue(true);
        }

        #region helpers
        private string CreateTempDir()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);

            return tempDir;
        }

        private void RemoveTempDir(string dir)
        {
            Directory.Delete(dir, true);
        }
        #endregion
    }
}
