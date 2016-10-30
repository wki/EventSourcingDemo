using System;
using System.IO;
using System.Linq;
using System.Threading;
using Akka.Actor;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using Wki.EventSourcing.Actors;
using Wki.EventSourcing.Messages;
using Wki.EventSourcing.Serialisation;
using Wki.EventSourcing.Tests.Messages;
using Wki.EventSourcing.Util;
using static Wki.EventSourcing.Tests.TempDir;

namespace Wki.EventSourcing.Tests
{
    public class FileJournalWriterTest : TestKit
    {
        private string storageDir;
        private IActorRef storageWriter;

        [SetUp]
        public void SetUp()
        {
            storageDir = CreateTempDir();
            storageWriter = Sys.ActorOf(Props.Create<FileJournalWriter>(storageDir), "writer");
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            Console.WriteLine($"Removing Temp dir: {storageDir}");
            RemoveTempDir(storageDir);
        }

        [Test]
        public void FileJournalWriter_AfterStart_DoesNotCreateFiles()
        {
            // Assert
            CollectionAssert.IsEmpty(Directory.EnumerateFileSystemEntries(storageDir));
        }

        [Test]
        [TestCase(2015,3,5, 1)]
        [TestCase(2013,5,3, 2)]
        public void FileJournalWriter_WhenPersisting_CreatesAndAppendsFile(int year, int month, int day, int nrMessages)
        {
            // Arrange
            var file = Path.Combine(storageDir, $"{year:D4}", $"{month:D2}", $"{day:D2}.json");
            var now = new DateTime(year, month, day, 11, 26, 0);
            SystemTime.Fake(() => now);

            // Act
            for (var i = 1; i <= nrMessages; i++)
                storageWriter.Tell(new PersistEvent(new SomethingHappened(i)));
            Thread.Sleep(TimeSpan.FromSeconds(0.5)); // avoid racing condition between writing and reading

            // Assert
            ExpectMsg<EventPersisted>(TimeSpan.FromSeconds(1.1));

            Assert.IsTrue(File.Exists(file), "File exists");
            Assert.AreEqual(nrMessages, File.ReadLines(file).Count(), "nr Lines");

            SystemTime.Fake(() => DateTime.Now);
            var events =
                File.ReadLines(file)
                    .Select(line => EventSerializer.FromJson(line))
                    .ToArray();

            Assert.IsTrue(events.All(e => e.OccuredOn == now), "Occured deserialized");
            Assert.IsTrue(((SomethingHappened)events[0]).Number == 1, "first is 1");
            Assert.IsTrue(((SomethingHappened)events.Last()).Number == nrMessages, $"last is ${nrMessages}");
        }

        [Test]
        [TestCase(2015, 3, 5, 1)]
        [TestCase(2013, 5, 3, 2)]
        public void FileJournalWriter_WhenPersisting_RepliesPersistedEvent(int year, int month, int day, int nrMessages)
        {
            // Arrange
            var now = new DateTime(year, month, day, 11, 26, 0);
            SystemTime.Fake(() => now);

            // Act
            for (var i = 1; i <= nrMessages; i++)
                storageWriter.Tell(new PersistEvent(new SomethingHappened(i)));
            Thread.Sleep(TimeSpan.FromSeconds(0.5)); // avoid racing condition between writing and reading

            // Assert
            for (var i = 1; i <= nrMessages; i++)
                ExpectMsg<EventPersisted>(p => ((SomethingHappened)p.Event).Number == i);
        }
    }
}
