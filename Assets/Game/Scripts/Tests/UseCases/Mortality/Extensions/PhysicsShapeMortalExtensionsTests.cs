using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;
using Zenject;

namespace Bw.Tests.UseCases.Mortality.Extensions
{
    public sealed class PhysicsShapeMortalExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private MortalFixture _fixture;
        private PhysicsBody _body;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _fixture = new MortalFixture();
            var space = new TestSpace(_lifetime.Lifetime, _objects);
            _body = space.Body(space.Box("level", Vector2.zero, Vector2.one, Vector2.zero), PhysicsBody.BodyType.Static);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            _fixture.Dispose();
        }

        [Test]
        public void ShapeOfAMortalObjectFindsItsMortal()
        {
            var owner = _objects.Create("character");
            var holder = owner.AddComponent<ReadonlyMortalHolder>();
            var container = new DiContainer();
            container.BindInstance<IReadonlyMortal>(_fixture.Mortal);
            container.Inject(holder);
            var shape = _body.CreateShape(Square(), PhysicsShapeDefinition.defaultDefinition, owner);

            var found = shape.TryGetMortal(out var mortal);

            Assert.That(found, Is.True);
            Assert.That(mortal, Is.SameAs(_fixture.Mortal));
        }

        [Test]
        public void ShapeOfAnObjectWithoutAMortalFindsNone()
        {
            var shape = _body.CreateShape(Square(), PhysicsShapeDefinition.defaultDefinition, _objects.Create("wall"));

            var found = shape.TryGetMortal(out var mortal);

            Assert.That(found, Is.False);
            Assert.That(mortal, Is.Null);
        }

        [Test]
        public void ShapeWithoutAnOwnerFindsNone()
        {
            var shape = _body.CreateShape(Square(), PhysicsShapeDefinition.defaultDefinition);

            Assert.That(shape.TryGetMortal(out _), Is.False);
        }

        private static PolygonGeometry Square() =>
            PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(Vector2.zero), false);
    }
}
