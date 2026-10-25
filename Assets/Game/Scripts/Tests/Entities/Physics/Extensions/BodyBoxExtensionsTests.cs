using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class BodyBoxExtensionsTests
    {
        private HiddenObjects _objects;

        [SetUp]
        public void SetUp() =>
            _objects = new HiddenObjects();

        [TearDown]
        public void TearDown() =>
            _objects.Dispose();

        [Test]
        public void WorldSizeIsTheSizeTimesTheAbsoluteScale()
        {
            var box = Box(new Vector2(2f, 1f), Vector2.zero);
            box.transform.localScale = new Vector3(2f, -3f, 1f);

            Assert.That(box.WorldSize(), Is.EqualTo(new Vector2(4f, 3f)));
        }

        [Test]
        public void WorldOffsetIsTheOffsetTimesTheSignedScale()
        {
            var box = Box(Vector2.one, new Vector2(0.5f, -1f));
            box.transform.localScale = new Vector3(2f, -3f, 1f);

            Assert.That(box.WorldOffset(), Is.EqualTo(new Vector2(1f, 3f)));
        }

        [Test]
        public void ParentScaleCounts()
        {
            var parent = _objects.Create("Parent");
            parent.transform.localScale = new Vector3(3f, 2f, 1f);
            var box = Box(new Vector2(1f, 1f), new Vector2(1f, 1f));
            box.transform.SetParent(parent.transform, false);

            Assert.That(box.WorldSize(), Is.EqualTo(new Vector2(3f, 2f)));
            Assert.That(box.WorldOffset(), Is.EqualTo(new Vector2(3f, 2f)));
        }

        [Test]
        public void WorldRectIsCentredOnThePositionPlusTheOffset()
        {
            var box = Box(new Vector2(2f, 1f), new Vector2(0.5f, -1f));
            box.transform.position = new Vector3(10f, 5f, 7f);

            Assert.That(box.WorldRect(), Is.EqualTo(new Rect(9.5f, 3.5f, 2f, 1f)));
        }

        [Test]
        public void LocalGeometryIgnoresThePlaceAndKeepsTheScaledOffset()
        {
            var box = Box(new Vector2(2f, 1f), new Vector2(1f, 0.5f));
            box.transform.SetPositionAndRotation(new Vector3(100f, 100f, 0f), Quaternion.Euler(0f, 0f, 30f));

            var bounds = box.LocalGeometry().CalculateAABB(new PhysicsTransform(Vector2.zero));

            AssertNear(bounds.lowerBound, new Vector2(0f, 0f));
            AssertNear(bounds.upperBound, new Vector2(2f, 1f));
        }

        [Test]
        public void WorldGeometryIsPlacedTurnedAndScaled()
        {
            var box = Box(new Vector2(2f, 1f), new Vector2(1f, 0f));
            box.transform.SetPositionAndRotation(new Vector3(3f, 4f, 0f), Quaternion.Euler(0f, 0f, 90f));

            var bounds = box.WorldGeometry().CalculateAABB(new PhysicsTransform(Vector2.zero));

            AssertNear(bounds.lowerBound, new Vector2(2.5f, 4f));
            AssertNear(bounds.upperBound, new Vector2(3.5f, 6f));
        }

        [Test]
        public void CategoriesAreTheLayerBit()
        {
            var box = Box(Vector2.one, Vector2.zero);
            box.gameObject.layer = 15;

            Assert.That(box.Categories().bitMask, Is.EqualTo(1ul << 15));
        }

        [Test]
        public void OneWayBoxAddsTheOneWayCategory()
        {
            var box = Box(Vector2.one, Vector2.zero).With("_oneWay", true);
            box.gameObject.layer = 14;

            Assert.That(box.Categories().bitMask, Is.EqualTo((1ul << 14) | (1ul << PhysicsCategories.OneWay)));
        }

        private BodyBox Box(Vector2 size, Vector2 offset) =>
            _objects.Create("Box").AddComponent<BodyBox>().With("_size", size).With("_offset", offset);

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(1e-4f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(1e-4f));
        }
    }
}
