using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class DynamicBodyConfigExtensionsTests
    {
        private PhysicsTestWorld _physics;
        private DynamicBodyConfig _config;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _config = ScriptableObject.CreateInstance<DynamicBodyConfig>();
            _config.GravityScale = 3f;
            _config.Contacts = (1 << 14) | (1 << 15);
            _config.Density = 2f;
            _config.Friction = 0.4f;
            _config.Bounciness = 0.25f;
            _config.LinearDamping = 0.1f;
            _config.AngularDamping = 0.2f;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_config);
            _physics.Dispose();
        }

        [Test]
        public void BodyDefinitionIsDynamicWithTheConfigsGravityAndDamping()
        {
            var definition = _config.BodyDefinition();

            Assert.That(definition.type, Is.EqualTo(PhysicsBody.BodyType.Dynamic));
            Assert.That(definition.gravityScale, Is.EqualTo(3f));
            Assert.That(definition.linearDamping, Is.EqualTo(0.1f));
            Assert.That(definition.angularDamping, Is.EqualTo(0.2f));
        }

        [Test]
        public void ShapeDefinitionCarriesTheMaterialAndTheContacts()
        {
            var definition = _config.ShapeDefinition();

            Assert.That(definition.density, Is.EqualTo(2f));
            Assert.That(definition.surfaceMaterial.friction, Is.EqualTo(0.4f));
            Assert.That(definition.surfaceMaterial.bounciness, Is.EqualTo(0.25f));
            Assert.That(definition.contactFilter.contacts.bitMask, Is.EqualTo((1ul << 14) | (1ul << 15)));
            Assert.That(definition.preSolveCallbacks, Is.True);
        }

        [Test]
        public void CreatedBodyHasTheBoxOnItsLayerAndASweepProbeAtItsCentre()
        {
            var box = _physics.Box("Crate", new Vector2(1f, 2f), Vector2.one, new Vector2(0f, 0.5f), PhysicsTestWorld.Ground);

            var body = _config.CreateBody(_physics.Space, _physics.Lifetime, box, PhysicsBody.BodyType.Kinematic);

            Assert.That(body.type, Is.EqualTo(PhysicsBody.BodyType.Kinematic));
            Assert.That(body.position, Is.EqualTo(new Vector2(1f, 2f)));
            Assert.That(body.gravityScale, Is.EqualTo(3f));
            Assert.That(body.fastCollisionsAllowed, Is.True);
            Assert.That(body.shapeCount, Is.EqualTo(2));
            using var shapes = body.GetShapes(Allocator.Temp);
            for (var index = 0; index < shapes.Length; index++)
            {
                var shape = shapes[index];
                if (shape.shapeType == PhysicsShape.ShapeType.Circle)
                    Assert.That(shape.circleGeometry.center, Is.EqualTo(new Vector2(0f, 0.5f)));
                else
                    Assert.That(shape.contactFilter.categories.bitMask, Is.EqualTo(1ul << PhysicsTestWorld.Ground));
            }
        }
    }
}
