using Bw.Entities.Physics.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class PolygonGeometryExtensionsTests
    {
        [TestCase(0f, 1.2f, 0f, 1f)]
        [TestCase(0f, -1.2f, 0f, -1f)]
        [TestCase(-1.3f, 0f, -1f, 0f)]
        [TestCase(1.3f, 0.4f, 1f, 0f)]
        public void NormalPointsFromTheOtherFaceTowardThePolygon(
            float x,
            float y,
            float normalX,
            float normalY)
        {
            var normal = Box(new Vector2(x, y), Vector2.one).NormalFrom(Box(Vector2.zero, Vector2.one));

            AssertNear(normal, new Vector2(normalX, normalY));
        }

        [Test]
        public void NearACornerTheMostSeparatingFaceWinsOverTheDiagonal()
        {
            var normal = Box(new Vector2(1.1f, 1.05f), Vector2.one).NormalFrom(Box(Vector2.zero, Vector2.one));

            AssertNear(normal, Vector2.right);
        }

        [Test]
        public void TurnedPolygonOverAFloorTakesTheFloorsFace()
        {
            var diamond = PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(new Vector2(0f, 1.5f), new PhysicsRotate(45f * Mathf.Deg2Rad)), false);

            var normal = diamond.NormalFrom(Box(Vector2.zero, new Vector2(10f, 1f)));

            AssertNear(normal, Vector2.up);
        }

        [Test]
        public void OverlappingPolygonsStillGetAFaceNormal()
        {
            var normal = Box(new Vector2(0f, 0.9f), Vector2.one).NormalFrom(Box(Vector2.zero, new Vector2(4f, 1f)));

            AssertNear(normal, Vector2.up);
        }

        [Test]
        public void DistanceInsideIsZero()
        {
            Assert.That(Box(Vector2.zero, Vector2.one).DistanceTo(new Vector2(0.2f, -0.3f)), Is.EqualTo(0f));
        }

        [TestCase(3f, 0f, 2.5f)]
        [TestCase(0f, -2f, 1.5f)]
        [TestCase(3.5f, 4.5f, 5f)]
        public void DistanceOutsideIsToTheNearestPoint(float x, float y, float distance)
        {
            Assert.That(Box(Vector2.zero, Vector2.one).DistanceTo(new Vector2(x, y)), Is.EqualTo(distance).Within(1e-4f));
        }

        [TestCase(1f, 0f, 1.5f)]
        [TestCase(-1f, 0f, 0.5f)]
        [TestCase(0f, 1f, 1f)]
        [TestCase(0f, -1f, 1f)]
        public void SeparationAlongIsHowFarThePolygonMustGoToClearTheOther(float x, float y, float separation)
        {
            var polygon = Box(Vector2.zero, Vector2.one);
            var other = Box(new Vector2(0.5f, 0f), Vector2.one);

            Assert.That(polygon.SeparationAlong(other, new Vector2(x, y)), Is.EqualTo(separation).Within(1e-5f));
        }

        [Test]
        public void SeparationAlongIsZeroWhenAlreadyClear()
        {
            var polygon = Box(Vector2.zero, Vector2.one);
            var other = Box(new Vector2(3f, 0f), Vector2.one);

            Assert.That(polygon.SeparationAlong(other, Vector2.left), Is.EqualTo(0f));
        }

        private static PolygonGeometry Box(Vector2 center, Vector2 size) =>
            PolygonGeometry.CreateBox(size, 0f, new PhysicsTransform(center), false);

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(1e-4f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(1e-4f));
        }
    }
}
