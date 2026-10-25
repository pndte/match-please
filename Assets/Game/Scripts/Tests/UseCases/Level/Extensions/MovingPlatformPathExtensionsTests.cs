using Bw.UseCases.Level;
using Bw.UseCases.Level.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Level.Extensions
{
    public sealed class MovingPlatformPathExtensionsTests
    {
        private static readonly MovingPlatformPath Path = new() { Travel = new Vector2(0f, 10f), TravelSeconds = 4f, PauseSeconds = 1f };

        [TestCase(0d)]
        [TestCase(0.5d)]
        [TestCase(0.999d)]
        public void PlatformWaitsAtTheStartFirst(double seconds)
        {
            Assert.That(Path.AlongAt(seconds), Is.Zero);
        }

        [TestCase(1d, 0f)]
        [TestCase(2d, 0.14644661f)]
        [TestCase(3d, 0.5f)]
        [TestCase(4d, 0.85355339f)]
        public void PlatformTravelsAwayEasedInAndOut(double seconds, float along)
        {
            Assert.That(Path.AlongAt(seconds), Is.EqualTo(along).Within(1e-6f));
        }

        [TestCase(5d)]
        [TestCase(5.5d)]
        [TestCase(5.999d)]
        public void PlatformWaitsAtTheEndOfTheTravel(double seconds)
        {
            Assert.That(Path.AlongAt(seconds), Is.EqualTo(1f));
        }

        [TestCase(6d, 1f)]
        [TestCase(7d, 0.85355339f)]
        [TestCase(8d, 0.5f)]
        [TestCase(9d, 0.14644661f)]
        public void PlatformComesBackTheSameWay(double seconds, float along)
        {
            Assert.That(Path.AlongAt(seconds), Is.EqualTo(along).Within(1e-6f));
        }

        [TestCase(0.3d)]
        [TestCase(2.7d)]
        [TestCase(5.2d)]
        [TestCase(8.9d)]
        public void PathRepeatsEveryTwoTravelsAndTwoPauses(double seconds)
        {
            Assert.That(Path.AlongAt(seconds + 10d), Is.EqualTo(Path.AlongAt(seconds)).Within(1e-6f));
            Assert.That(Path.AlongAt(seconds + 1000d), Is.EqualTo(Path.AlongAt(seconds)).Within(1e-5f));
        }

        [Test]
        public void SecondsBeforeZeroFollowTheSameCycle()
        {
            Assert.That(Path.AlongAt(-2d), Is.EqualTo(Path.AlongAt(8d)).Within(1e-6f));
        }

        [Test]
        public void TravelAwayNeverTurnsBack()
        {
            var previous = Path.AlongAt(1d);
            for (var seconds = 1.05d; seconds < 5d; seconds += 0.05d)
            {
                var along = Path.AlongAt(seconds);
                Assert.That(along, Is.GreaterThanOrEqualTo(previous));
                previous = along;
            }
        }

        [Test]
        public void ShareStaysWithinTheRail()
        {
            for (var seconds = 0d; seconds < 20d; seconds += 0.01d)
                Assert.That(Path.AlongAt(seconds), Is.InRange(0f, 1f));
        }

        [Test]
        public void PathWithoutPausesTurnsAtOnce()
        {
            var path = new MovingPlatformPath { Travel = Vector2.one, TravelSeconds = 2f, PauseSeconds = 0f };

            Assert.That(path.AlongAt(0d), Is.Zero);
            Assert.That(path.AlongAt(2d), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(path.AlongAt(3d), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(path.AlongAt(4d), Is.Zero.Within(1e-6f));
        }
    }
}
