using Bw.Tests.Entities.Physics.Doubles;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics
{
    public sealed class OneWayContactsTests
    {
        private static readonly Vector2 PlankAt = new(0f, 5f);
        private static readonly Vector2 PlankSize = new(4f, 0.2f);
        private const float PlankTop = 5.1f;
        private const float PlankBottom = 4.9f;
        private const float HalfCrate = 0.5f;

        private PhysicsTestWorld _physics;

        [SetUp]
        public void SetUp() =>
            _physics = new PhysicsTestWorld();

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void BodyDroppedOnAPlankRestsOnIt()
        {
            _physics.StaticPlank("Plank", PlankAt, PlankSize);
            var crate = _physics.Dynamic("Crate", new Vector2(0f, 7f), Vector2.one);

            _physics.Step(60);

            Assert.That(crate.position.y, Is.EqualTo(PlankTop + HalfCrate).Within(0.02f));
        }

        [Test]
        public void BodyRisingFromBelowPassesThroughAPlank()
        {
            _physics.StaticPlank("Plank", PlankAt, PlankSize);
            var crate = _physics.Dynamic("Crate", new Vector2(0f, 3f), Vector2.one);
            crate.linearVelocity = new Vector2(0f, 15f);

            _physics.Step(10);

            Assert.That(crate.position.y - HalfCrate, Is.GreaterThan(PlankTop));
        }

        [Test]
        public void BodyThatPassedFromBelowLandsOnThePlanksTop()
        {
            _physics.StaticPlank("Plank", PlankAt, PlankSize);
            var crate = _physics.Dynamic("Crate", new Vector2(0f, 3f), Vector2.one);
            crate.linearVelocity = new Vector2(0f, 15f);

            _physics.Step(120);

            Assert.That(crate.position.y, Is.EqualTo(PlankTop + HalfCrate).Within(0.02f));
        }

        [Test]
        public void SolidBoxStopsABodyRisingFromBelow()
        {
            _physics.Static("Beam", PlankAt, PlankSize);
            var crate = _physics.Dynamic("Crate", new Vector2(0f, 3f), Vector2.one);
            crate.linearVelocity = new Vector2(0f, 15f);

            _physics.Step(10);

            Assert.That(crate.position.y + HalfCrate, Is.LessThanOrEqualTo(PlankBottom + 0.01f));
        }

        [Test]
        public void ShapeIsOneWayOnlyWhileItsLifetimeLasts()
        {
            var beam = _physics.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static });
            var shape = beam.CreateShape(PolygonGeometry.CreateBox(PlankSize, 0f, new PhysicsTransform(PlankAt), false), PhysicsShapeDefinition.defaultDefinition);
            var oneWay = new LifetimeDefinition();
            _physics.Space.MakeOneWay(oneWay.Lifetime, shape);
            var crate = _physics.Dynamic("Crate", new Vector2(0f, 3f), Vector2.one);
            crate.linearVelocity = new Vector2(0f, 15f);

            oneWay.Terminate();
            _physics.Step(10);

            Assert.That(crate.position.y + HalfCrate, Is.LessThanOrEqualTo(PlankBottom + 0.01f));
        }
    }
}
