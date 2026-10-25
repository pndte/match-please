using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Extensions
{
    public sealed class PhysicsWorldExtensionsTests
    {
        private static readonly PhysicsQuery.QueryFilter Ground = new(PhysicsMask.All, new PhysicsMask { bitMask = 1ul << PhysicsTestWorld.Ground });

        private PhysicsTestWorld _physics;

        [SetUp]
        public void SetUp() =>
            _physics = new PhysicsTestWorld();

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void EmptySpotIsFree()
        {
            _physics.Static("Wall", new Vector2(5f, 0f), Vector2.one);

            Assert.That(_physics.World.IsFree(BoxAt(Vector2.zero), Ground), Is.True);
        }

        [Test]
        public void SpotOverlappingABoxIsNotFree()
        {
            _physics.Static("Wall", new Vector2(0.8f, 0f), Vector2.one);

            Assert.That(_physics.World.IsFree(BoxAt(Vector2.zero), Ground), Is.False);
        }

        [Test]
        public void BoxOutsideTheFilterLeavesTheSpotFree()
        {
            _physics.Space.CreateBody(_physics.Lifetime, _physics.Box("Character", Vector2.zero, Vector2.one, Vector2.zero, PhysicsTestWorld.Character), PhysicsBody.BodyType.Static);

            Assert.That(_physics.World.IsFree(BoxAt(Vector2.zero), Ground), Is.True);
        }

        [Test]
        public void SolidOverlapIsNotFreeOfSolids()
        {
            _physics.Static("Wall", new Vector2(0.8f, 0f), Vector2.one);
            var ignored = _physics.Static("Other", new Vector2(50f, 0f), Vector2.one);

            Assert.That(_physics.World.IsFreeOfSolids(BoxAt(Vector2.zero), Ground, ignored), Is.False);
        }

        [Test]
        public void IgnoredBodyIsNoSolid()
        {
            var ignored = _physics.Static("Holder", new Vector2(0.8f, 0f), Vector2.one);

            Assert.That(_physics.World.IsFreeOfSolids(BoxAt(Vector2.zero), Ground, ignored), Is.True);
        }

        [Test]
        public void PlankIsNoSolid()
        {
            _physics.StaticPlank("Plank", new Vector2(0f, 0.3f), new Vector2(4f, 0.2f));
            var ignored = _physics.Static("Other", new Vector2(50f, 0f), Vector2.one);

            Assert.That(_physics.World.IsFreeOfSolids(BoxAt(Vector2.zero), Ground, ignored), Is.True);
        }

        private static PolygonGeometry BoxAt(Vector2 center) =>
            PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(center), false);
    }
}
