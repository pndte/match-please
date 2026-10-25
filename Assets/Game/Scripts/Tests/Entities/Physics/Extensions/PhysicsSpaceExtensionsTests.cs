using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class PhysicsSpaceExtensionsTests
    {
        private PhysicsTestWorld _physics;

        [SetUp]
        public void SetUp() =>
            _physics = new PhysicsTestWorld();

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void BodyStandsWhereTheBoxStandsTurnedLikeIt()
        {
            var box = _physics.Box("Box", new Vector2(3f, 4f), new Vector2(2f, 1f));
            box.transform.rotation = Quaternion.Euler(0f, 0f, 30f);

            var body = _physics.Space.CreateBody(_physics.Lifetime, box, PhysicsBody.BodyType.Kinematic);

            Assert.That(body.type, Is.EqualTo(PhysicsBody.BodyType.Kinematic));
            Assert.That(body.position, Is.EqualTo(new Vector2(3f, 4f)));
            Assert.That(body.rotation.angle * Mathf.Rad2Deg, Is.EqualTo(30f).Within(1e-3f));
        }

        [Test]
        public void BodyHasOneShapeOfTheBoxOwnedByItsObject()
        {
            var box = _physics.Box("Box", new Vector2(3f, 4f), new Vector2(2f, 1f), new Vector2(0f, 0.5f), PhysicsTestWorld.Character);

            var body = _physics.Space.CreateBody(_physics.Lifetime, box, PhysicsBody.BodyType.Dynamic);

            var shape = OnlyShapeOf(body);
            Assert.That(shape.userData.objectValue, Is.SameAs(box.gameObject));
            Assert.That(shape.contactFilter.categories.bitMask, Is.EqualTo(1ul << PhysicsTestWorld.Character));
            var bounds = shape.polygonGeometry.CalculateAABB(body.transform);
            Assert.That(bounds.lowerBound.x, Is.EqualTo(2f).Within(1e-3f));
            Assert.That(bounds.lowerBound.y, Is.EqualTo(4f).Within(1e-3f));
            Assert.That(bounds.upperBound.x, Is.EqualTo(4f).Within(1e-3f));
            Assert.That(bounds.upperBound.y, Is.EqualTo(5f).Within(1e-3f));
        }

        [Test]
        public void OneWayBoxMakesAOneWayShape()
        {
            var shape = OnlyShapeOf(_physics.Space.CreateBody(_physics.Lifetime, _physics.Plank("Plank", Vector2.zero, new Vector2(4f, 0.2f)), PhysicsBody.BodyType.Static));

            Assert.That(shape.IsOneWay(), Is.True);
            Assert.That(shape.preSolveCallbacks, Is.True);
        }

        [Test]
        public void SolidBoxMakesNoOneWayShape()
        {
            var shape = OnlyShapeOf(_physics.Static("Beam", Vector2.zero, new Vector2(4f, 0.2f)));

            Assert.That(shape.IsOneWay(), Is.False);
        }

        [Test]
        public void DefinitionsAreKeptButTheCategoriesComeFromTheBox()
        {
            var box = _physics.Box("Box", Vector2.zero, Vector2.one, Vector2.zero, PhysicsTestWorld.Character);
            var definition = PhysicsBodyDefinition.defaultDefinition;
            definition.type = PhysicsBody.BodyType.Dynamic;
            definition.gravityScale = 3f;
            var shapeDefinition = PhysicsShapeDefinition.defaultDefinition;
            shapeDefinition.density = 2f;
            shapeDefinition.contactFilter = new PhysicsShape.ContactFilter(PhysicsMask.All, new PhysicsMask { bitMask = 1ul << 14 }, 0);

            var body = _physics.Space.CreateBody(_physics.Lifetime, box, definition, shapeDefinition);

            var shape = OnlyShapeOf(body);
            Assert.That(body.gravityScale, Is.EqualTo(3f));
            Assert.That(shape.GetDensity(), Is.EqualTo(2f));
            Assert.That(shape.contactFilter.categories.bitMask, Is.EqualTo(1ul << PhysicsTestWorld.Character));
            Assert.That(shape.contactFilter.contacts.bitMask, Is.EqualTo(1ul << 14));
        }

        [Test]
        public void BodyDiesWithTheLifetime()
        {
            var lifetime = new LifetimeDefinition();
            var body = _physics.Space.CreateBody(lifetime.Lifetime, _physics.Box("Box", Vector2.zero, Vector2.one), PhysicsBody.BodyType.Dynamic);

            lifetime.Terminate();

            Assert.That(body.isValid, Is.False);
        }

        [Test]
        public void BodyDestroyedBeforeItsLifetimeEndsIsLeftAlone()
        {
            var lifetime = new LifetimeDefinition();
            var body = _physics.Space.CreateBody(lifetime.Lifetime, _physics.Box("Box", Vector2.zero, Vector2.one), PhysicsBody.BodyType.Dynamic);
            body.Destroy(0);

            Assert.DoesNotThrow(() => lifetime.Terminate());
        }

        private static PhysicsShape OnlyShapeOf(PhysicsBody body)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            Assert.That(shapes.Length, Is.EqualTo(1));
            return shapes[0];
        }
    }
}
