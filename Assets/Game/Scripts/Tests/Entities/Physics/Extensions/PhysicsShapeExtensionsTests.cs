using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class PhysicsShapeExtensionsTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static });
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [TestCase(0)]
        [TestCase(14)]
        [TestCase(31)]
        public void LayerIsTheOneLayerBit(int layer)
        {
            var shape = Shape(1ul << layer);

            Assert.That(shape.Layer(), Is.EqualTo(layer));
        }

        [Test]
        public void LayerIgnoresThePhysicsOnlyCategories()
        {
            var shape = Shape((1ul << 14) | (1ul << PhysicsCategories.OneWay));

            Assert.That(shape.Layer(), Is.EqualTo(14));
        }

        [Test]
        public void ShapeWithoutALayerThrows()
        {
            var shape = Shape(1ul << PhysicsCategories.OneWay);

            Assert.Throws<InvalidOperationException>(() => shape.Layer());
        }

        [Test]
        public void ShapeOnTwoLayersThrows()
        {
            var shape = Shape((1ul << 14) | (1ul << 15));

            Assert.Throws<InvalidOperationException>(() => shape.Layer());
        }

        [Test]
        public void ComponentIsFoundOnTheOwner()
        {
            var owner = _physics.Objects.Create("Owner");
            var box = owner.AddComponent<BodyBox>();
            var shape = _body.CreateShape(UnitBox(Vector2.zero), PhysicsShapeDefinition.defaultDefinition, owner);

            Assert.That(shape.TryGetComponent<BodyBox>(out var found), Is.True);
            Assert.That(found, Is.SameAs(box));
        }

        [Test]
        public void OwnerWithoutTheComponentFindsNothing()
        {
            var shape = _body.CreateShape(UnitBox(Vector2.zero), PhysicsShapeDefinition.defaultDefinition, _physics.Objects.Create("Owner"));

            Assert.That(shape.TryGetComponent<BodyBox>(out var found), Is.False);
            Assert.That(found, Is.Null);
        }

        [Test]
        public void ShapeWithoutAnOwnerFindsNothing()
        {
            var shape = Shape(1ul);

            Assert.That(shape.TryGetComponent<BodyBox>(out _), Is.False);
        }

        [Test]
        public void OneWayIsTheOneWayCategory()
        {
            Assert.That(Shape((1ul << 14) | (1ul << PhysicsCategories.OneWay)).IsOneWay(), Is.True);
            Assert.That(Shape(1ul << 14).IsOneWay(), Is.False);
        }

        [Test]
        public void DistanceInsideIsZero()
        {
            var shape = _body.CreateShape(UnitBox(new Vector2(2f, 0f)), PhysicsShapeDefinition.defaultDefinition);

            Assert.That(shape.DistanceTo(new Vector2(2.2f, 0.1f)), Is.EqualTo(0f));
        }

        [TestCase(5f, 0f, 2.5f)]
        [TestCase(2f, -3f, 2.5f)]
        [TestCase(5.5f, 4.5f, 5f)]
        public void DistanceOutsideIsToTheNearestPointOfTheShape(float x, float y, float distance)
        {
            var shape = _body.CreateShape(UnitBox(new Vector2(2f, 0f)), PhysicsShapeDefinition.defaultDefinition);

            Assert.That(shape.DistanceTo(new Vector2(x, y)), Is.EqualTo(distance).Within(1e-4f));
        }

        private PhysicsShape Shape(ulong categories)
        {
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.contactFilter = new PhysicsShape.ContactFilter(new PhysicsMask { bitMask = categories }, PhysicsMask.All, 0);
            return _body.CreateShape(UnitBox(Vector2.zero), definition);
        }

        private static PolygonGeometry UnitBox(Vector2 center) =>
            PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(center), false);
    }
}
