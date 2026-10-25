using Bw.UseCases.Level;
using Bw.UseCases.Level.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Level.Extensions
{
    public sealed class PlatformRailExtensionsTests
    {
        private static readonly PlatformRail Rail = new(new Vector2(2f, 3f), new Vector2(0f, 10f));

        [TestCase(0f, 2f, 3f)]
        [TestCase(1f, 2f, 13f)]
        [TestCase(0.25f, 2f, 5.5f)]
        [TestCase(-0.5f, 2f, -2f)]
        [TestCase(1.5f, 2f, 18f)]
        public void PositionIsTheStartPlusTheShareOfTheTravel(float along, float x, float y)
        {
            Assert.That(Rail.PositionAt(along), Is.EqualTo(new Vector2(x, y)));
        }

        [TestCase(0f)]
        [TestCase(0.37f)]
        [TestCase(1f)]
        [TestCase(-0.2f)]
        public void ShareOfAPositionOnTheRailComesBack(float along)
        {
            Assert.That(Rail.AlongAt(Rail.PositionAt(along)), Is.EqualTo(along).Within(1e-6f));
        }

        [Test]
        public void PositionBesideTheRailIsProjectedOntoIt()
        {
            Assert.That(Rail.AlongAt(new Vector2(40f, 8f)), Is.EqualTo(0.5f).Within(1e-6f));
        }

        [Test]
        public void DiagonalRailMeasuresTheShareAlongItsDirection()
        {
            var rail = new PlatformRail(Vector2.zero, new Vector2(3f, 4f));

            Assert.That(rail.AlongAt(new Vector2(3f, 4f)), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(rail.AlongAt(new Vector2(1.5f, 2f)), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(rail.AlongAt(new Vector2(4f, -3f)), Is.EqualTo(0f).Within(1e-6f));
        }

        [TestCase(0f, 3f)]
        [TestCase(1f, 13f)]
        [TestCase(0.5f, 8f)]
        public void PositionBetweenTwoStatesFollowsTheProgress(float progress, float y)
        {
            Assert.That(Rail.PositionAt(new PlatformState(0f), new PlatformState(1f), progress), Is.EqualTo(new Vector2(2f, y)));
        }

        [Test]
        public void ProgressBeyondTheStatesIsClamped()
        {
            Assert.That(Rail.PositionAt(new PlatformState(0.2f), new PlatformState(0.4f), 3f), Is.EqualTo(Rail.PositionAt(0.4f)));
            Assert.That(Rail.PositionAt(new PlatformState(0.2f), new PlatformState(0.4f), -1f), Is.EqualTo(Rail.PositionAt(0.2f)));
        }
    }
}
