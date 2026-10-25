using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class PhysicsBodyExtensionsTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Dynamic });
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void CreatedShapeCarriesItsOwner()
        {
            var owner = _physics.Objects.Create("Owner");

            var shape = _body.CreateShape(UnitBox(), PhysicsShapeDefinition.defaultDefinition, owner);

            Assert.That(shape.userData.objectValue, Is.SameAs(owner));
            Assert.That(shape.body, Is.EqualTo(_body));
        }

        [Test]
        public void MoveToLayerSwapsTheLayerBitAndKeepsThePhysicsOnlyCategories()
        {
            var shape = _body.CreateShape(UnitBox(), Categorized((1ul << 14) | (1ul << PhysicsCategories.OneWay)));

            _body.MoveToLayer(17);

            Assert.That(shape.contactFilter.categories.bitMask, Is.EqualTo((1ul << 17) | (1ul << PhysicsCategories.OneWay)));
        }

        [Test]
        public void MoveToLayerMovesEveryShapeOfTheBody()
        {
            var first = _body.CreateShape(UnitBox(), Categorized(1ul << 14));
            var second = _body.CreateShape(UnitBox(), Categorized(1ul << 15));

            _body.MoveToLayer(3);

            Assert.That(first.contactFilter.categories.bitMask, Is.EqualTo(1ul << 3));
            Assert.That(second.contactFilter.categories.bitMask, Is.EqualTo(1ul << 3));
        }

        [Test]
        public void MoveToLayerLeavesShapesWithoutCategoriesAlone()
        {
            var shape = _body.CreateShape(UnitBox(), Categorized(0ul));

            _body.MoveToLayer(15);

            Assert.That(shape.contactFilter.categories.bitMask, Is.EqualTo(0ul));
        }

        [Test]
        public void SweepContinuouslyAddsAWeightlessProbeThatTouchesNothing()
        {
            _body.CreateShape(UnitBox(), PhysicsShapeDefinition.defaultDefinition);
            var mass = _body.mass;

            _body.SweepContinuously(new Vector2(0.25f, -0.5f));

            Assert.That(_body.fastCollisionsAllowed, Is.True);
            Assert.That(_body.shapeCount, Is.EqualTo(2));
            Assert.That(_body.mass, Is.EqualTo(mass).Within(1e-6f));
            var probe = CircleOf(_body);
            Assert.That(probe.circleGeometry.center, Is.EqualTo(new Vector2(0.25f, -0.5f)));
            Assert.That(probe.circleGeometry.radius, Is.EqualTo(0.01f).Within(1e-6f));
            Assert.That(probe.contactFilter.categories.bitMask, Is.EqualTo(0ul));
            Assert.That(probe.contactFilter.contacts.bitMask, Is.EqualTo(0ul));
        }

        private static PhysicsShape CircleOf(PhysicsBody body)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            for (var index = 0; index < shapes.Length; index++)
                if (shapes[index].shapeType == PhysicsShape.ShapeType.Circle)
                    return shapes[index];

            throw new AssertionException("The body has no circle shape.");
        }

        private static PolygonGeometry UnitBox() =>
            PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(Vector2.zero), false);

        private static PhysicsShapeDefinition Categorized(ulong categories)
        {
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.contactFilter = new PhysicsShape.ContactFilter(new PhysicsMask { bitMask = categories }, PhysicsMask.All, 0);
            return definition;
        }
    }
}
