using System;
using System.IO;
using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.NUnit;
using NUnit.Framework;
using Wki.EventSourcing.Actors;
using static Wki.EventSourcing.Tests.TempDir;

namespace Wki.EventSourcing.Tests
{
    public class SimpleStorageActor : FileJournalBase
    {
        public SimpleStorageActor(string storageDir) : base(storageDir)
        {
            // might force directory creation
            Receive<DateTime>(d => Sender.Tell(Dir(d)));
        }
    }

    [TestFixture]
    public class FileJournalBaseTest : TestKit
    {
        private string storageDir;
        private IActorRef storageActor;

        [SetUp]
        public void SetUp()
        {
            storageDir = CreateTempDir();
            storageActor = Sys.ActorOf(Props.Create<SimpleStorageActor>(storageDir), "storage");
        }

        [TearDown]
        public void TestFixtureTearDown()
        {
            RemoveTempDir(storageDir);
        }

        [Test]
        public void FileStorage_WithoutAnyAction_DoesNotCreateDirectories()
        {
            // Assert
            CollectionAssert.IsEmpty(Directory.EnumerateFileSystemEntries(storageDir));
        }

        [Test]
        [TestCase(2015,3,5)]
        [TestCase(2013,5,3)]
        public void FileStorage_AccessingDir_CreatesMissingDirectory(int year, int month, int day)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            storageActor.Tell(date);

            // Assert
            ExpectMsg<string>(s => Directory.Exists(s));
        }
    
        [Test]
        [TestCase(2015, 3, 5)]
        [TestCase(2013, 5, 3)]
        public void FileStorage_AccessingDir_DirectoryNamesMonth(int year, int month, int day)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            storageActor.Tell(date);

            // Assert
            ExpectMsg<string>(s => Path.GetFileName(s) == $"{month:D2}");
        }

        [Test]
        [TestCase(2015, 3, 5)]
        [TestCase(2013, 5, 3)]
        public void FileStorage_AccessingDir_ParentDirectoryNamesYear(int year, int month, int day)
        {
            // Arrange
            var date = new DateTime(year, month, day);

            // Act
            storageActor.Tell(date);

            // Assert
            ExpectMsg<string>(s => Path.GetFileName(Path.GetDirectoryName(s)) == $"{year:D4}");
        }

        [Test]
        [TestCase(2015, 3, 5)]
        [TestCase(2013, 5, 3)]
        public void FileStorage_AccessingDir_RootDirectoryNamesYear(int year, int month, int day)
        {
            // Arrange
            var date = new DateTime(year, month, day);
            var yearDir = Path.Combine(storageDir, $"{year:D4}");

            // Act
            storageActor.Tell(date);

            // Assert
            ExpectMsg<string>(s => Directory.Exists(yearDir));
        }
    }
}
