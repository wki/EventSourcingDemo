using System;
using NUnit.Framework;
using Akka.TestKit.NUnit;
using Akka.Actor;
using static Wki.EventSourcing.Tests.TempDir;
using Wki.EventSourcing.Protocol;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Tests.Messages;
using System.IO;
using Wki.EventSourcing.Serialisation;
using Wki.EventSourcing.Util;
using Wki.EventSourcing.Protocol.EventStore;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class FileJournalReaderTest : TestKit
    {
        private string storageDir;
        private IActorRef journalReader;

        [SetUp]
        public void SetUp()
        {
            storageDir = CreateTempDir();
            journalReader = Sys.ActorOf(Props.Create<FileJournalReader>(storageDir), "reader");
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            Console.WriteLine($"Removing Temp dir: {storageDir}");
            RemoveTempDir(storageDir);
        }

        [Test]
        public void FileJournalReader_Initially_SendsNoMessage()
        {
            // Assert
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void FileJournalReader_EmptyDir_RestoresNoEvent()
        {
            // Act
            journalReader.Tell(new LoadNextEvents(100));

            // Assert
            ExpectMsg<EndOfTransmission>();
        }

        [Test]
        public void FileJournalReader_FilledDir_ReturnsWantedNrOfEvents()
        {
            // Arrange
            var day = new DateTime(2015, 3, 5, /**/ 20, 1, 42);
            SystemTime.Fake(() => day);

            for (int i = 1; i < 10; i++)
                AppendEvent(new SomethingHappened(i));

            SystemTime.Fake(() => DateTime.Now);

            // Act
            journalReader.Tell(new LoadNextEvents(5));

            // Assert
            for (var i = 1; i <= 5; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            // only read the 5 requested messages then stop
            ExpectNoMsg(TimeSpan.FromSeconds(0.5));

            // -----------------

            // SECOND Act
            journalReader.Tell(new LoadNextEvents(99));

            // SECOND Assert
            for (var i = 6; i < 10; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            ExpectMsg<EndOfTransmission>();
        }

        [Test]
        public void FileJournalReader_FilledDirWithManyFiles_ReturnsEventsInOrder()
        { 
            // Arrange: 1-3: 28.2.2015   -- written third
            // Arrange: 4-6: 5.3.2015    -- written first
            // Arrange: 7-10: 1.7.2015   -- written second
            SystemTime.Fake(() => new DateTime(2015, 3, 5));
            for (int i = 4; i <= 6; i++)
                AppendEvent(new SomethingHappened(i));

            SystemTime.Fake(() => new DateTime(2015, 7, 1));
            for (int i = 7; i <= 10; i++)
                AppendEvent(new SomethingHappened(i));

            SystemTime.Fake(() => new DateTime(2015, 2, 28));
            for (int i = 1; i <= 3; i++)
                AppendEvent(new SomethingHappened(i));

            SystemTime.Fake(() => DateTime.Now);

            // Act
            journalReader.Tell(new LoadNextEvents(99));

            // Assert
            for (var i = 1; i <= 10; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            ExpectMsg<EndOfTransmission>();
        }

        [Test]
        public void FileJournalReader_FilledDirWithManyFiles_StopsAfterRequestedAmount()
        {
            // Arrange
            for (int i = 1; i <= 150; i++)
                AppendEvent(new SomethingHappened(i));

            // Act
            journalReader.Tell(new LoadNextEvents(50));

            // Assert
            for (var i = 1; i <= 50; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            ExpectNoMsg(TimeSpan.FromSeconds(0.5));
        }

        [Test]
        public void FileJournalReader_FilledDirWithManyFiles_ContinuesAfterRequestedAmount()
        {
            // Arrange
            for (int i = 1; i <= 150; i++)
                AppendEvent(new SomethingHappened(i));

            // Act
            journalReader.Tell(new LoadNextEvents(100));
            for (var i = 1; i <= 100; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            journalReader.Tell(new LoadNextEvents(100));

            // Assert
            for (var i = 101; i <= 150; i++)
                ExpectMsg<EventLoaded>(e => ((SomethingHappened)e.Event).Number == i);

            ExpectMsg<EndOfTransmission>();
        }

        #region helper
        private void AppendEvent(Event @event)
        {
            var day = @event.OccuredOn;
            var file = Path.Combine(storageDir, $"{day.Year:D4}", $"{day.Month:D2}", $"{day.Day:D2}.json");
            var dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.AppendAllLines(file, new[] { EventSerializer.ToJson(@event) });
        }
        #endregion
    }
}
