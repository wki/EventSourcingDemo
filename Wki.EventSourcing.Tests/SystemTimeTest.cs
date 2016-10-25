using System;
using NUnit.Framework;
using Wki.EventSourcing.Util;

namespace Wki.EventSourcing.Tests
{
    [TestFixture]
    public class SystemTimeTest
    {
        [Test]
        public void SystemTime_Unfaked_ReturnsCurrentTime()
        {
            // Arrange
            var currentTime = DateTime.Now;

            // Act
            var systemTime = SystemTime.Now;

            // Assert
            Assert.IsTrue(systemTime - currentTime < TimeSpan.FromSeconds(1));
        }

        [Test]
        public void SystemTime_Faked_ReturnsFakedTime()
        {
            // Arrange
            var fakeTime = new DateTime(1964, 3, 5, /**/ 19, 42, 0);
            SystemTime.Fake(() => fakeTime);

            // Act
            var systemTime = SystemTime.Now;

            // Assert
            Assert.AreEqual(fakeTime, systemTime);
        }
    }
}
