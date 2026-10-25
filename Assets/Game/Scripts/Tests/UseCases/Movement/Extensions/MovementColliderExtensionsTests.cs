using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class MovementColliderExtensionsTests
    {
        private ScriptedCollider _collider;

        [SetUp]
        public void SetUp() =>
            _collider = new ScriptedCollider();

        [Test]
        public void OffsetInTheOpenIsReachedWhole()
        {
            var reachable = _collider.ReachableOffset(Vector2.zero, new Vector2(0.7f, -0.4f));

            AssertNear(reachable, new Vector2(0.7f, -0.4f));
        }

        [TestCase(1f, 0.3f)]
        [TestCase(-1f, -0.2f)]
        public void WallStopsTheSidewaysPart(float x, float reached)
        {
            _collider.Right = 0.3f;
            _collider.Left = -0.2f;

            var reachable = _collider.ReachableOffset(Vector2.zero, new Vector2(x, 0.5f));

            AssertNear(reachable, new Vector2(reached, 0.5f));
        }

        [Test]
        public void CeilingStopsTheUpwardPart()
        {
            _collider.Ceiling = 0.25f;

            var reachable = _collider.ReachableOffset(Vector2.zero, new Vector2(0f, 1f));

            AssertNear(reachable, new Vector2(0f, 0.25f));
            Assert.That(_collider.Calls, Is.EqualTo(new[] { "Cast up" }));
        }

        [Test]
        public void DownwardPartFallsAndLandsOnAPlank()
        {
            _collider.Plank = -0.3f;

            var reachable = _collider.ReachableOffset(Vector2.zero, new Vector2(0f, -1f));

            AssertNear(reachable, new Vector2(0f, -0.3f));
            Assert.That(_collider.Calls, Is.EqualTo(new[] { "Fall" }));
        }

        [Test]
        public void VerticalPartStartsWhereTheSidewaysPartEnded()
        {
            _collider.Floor = -0.2f;
            _collider.LedgeX = 0f;

            var reachable = _collider.ReachableOffset(new Vector2(-0.1f, 0f), new Vector2(0.5f, -1f));

            AssertNear(reachable, new Vector2(0.5f, -1f));
        }

        [Test]
        public void NoOffsetCastsNothing()
        {
            var reachable = _collider.ReachableOffset(Vector2.one, Vector2.zero);

            Assert.That(reachable, Is.EqualTo(Vector2.zero));
            Assert.That(_collider.Calls, Is.Empty);
        }

        [Test]
        public void DriftIsKeptAlongTheAxesReachedWhole()
        {
            Assert.That(new Vector2(3f, -2f).ReachableDrift(new Vector2(1f, -1f), new Vector2(1f, -1f)), Is.EqualTo(new Vector2(3f, -2f)));
        }

        [Test]
        public void DriftIsDroppedAlongABlockedAxis()
        {
            Assert.That(new Vector2(3f, -2f).ReachableDrift(new Vector2(1f, -1f), new Vector2(0.4f, -1f)), Is.EqualTo(new Vector2(0f, -2f)));
            Assert.That(new Vector2(3f, -2f).ReachableDrift(new Vector2(1f, -1f), new Vector2(1f, -0.5f)), Is.EqualTo(new Vector2(3f, 0f)));
        }

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(1e-5f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(1e-5f));
        }
    }
}
