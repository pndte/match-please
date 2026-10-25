using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Props;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Props
{
    public sealed class ThrowTrajectoryTests
    {
        private const float Horizon = 1.2f;

        private PhysicsTestWorld _world;
        private HeldPropConfig _held;
        private DynamicBodyConfig _body;
        private ThrowTrajectory _trajectory;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _held = ScriptableObject.CreateInstance<HeldPropConfig>();
            _body = ScriptableObject.CreateInstance<DynamicBodyConfig>();
            var shape = _world.Box("Crate", new Vector2(0f, 50f), Vector2.one, new Vector2(0f, 0.5f), 16);
            _trajectory = new ThrowTrajectory(_world.Space, shape, _held, _body);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_body);
            Object.DestroyImmediate(_held);
            _world.Dispose();
        }

        [Test]
        public void GravityIsTheWorldsGravityScaledForTheBody()
        {
            Assert.That(_trajectory.Gravity, Is.EqualTo(Physics2D.gravity * _body.GravityScale));
        }

        [TestCase(0f, 0f, 0.5f)]
        [TestCase(90f, -0.5f, 0f)]
        [TestCase(180f, 0f, -0.5f)]
        [TestCase(-90f, 0.5f, 0f)]
        public void CenterIsTheShapeOffsetTurnedWithTheBody(float angle, float x, float y)
        {
            var center = _trajectory.CenterOf(new RigidState(new Vector2(3f, 4f), angle, Vector2.zero, 0f));

            Assert.That((center - new Vector2(3f + x, 4f + y)).magnitude, Is.LessThan(1e-5f));
        }

        [Test]
        public void OpenFlightIsClearToTheHorizon()
        {
            Assert.That(_trajectory.ClearTime(Thrown(new Vector2(10f, 5f)), Horizon), Is.EqualTo(Horizon));
        }

        [Test]
        public void FlightEndsWhereItMeetsAWall()
        {
            _world.Static("Wall", new Vector2(5.5f, 0f), new Vector2(1f, 30f));

            Assert.That(_trajectory.ClearTime(Thrown(new Vector2(10f, 0f)), Horizon), Is.EqualTo(0.5f).Within(1e-3f));
        }

        [Test]
        public void WallBehindTheThrowDoesNotEndTheFlight()
        {
            _world.Static("Wall", new Vector2(-5.5f, 0f), new Vector2(1f, 30f));

            Assert.That(_trajectory.ClearTime(Thrown(new Vector2(10f, 0f)), Horizon), Is.EqualTo(Horizon));
        }

        [Test]
        public void OneWayPlankDoesNotEndTheFlight()
        {
            _world.StaticPlank("Plank", new Vector2(3f, -2f), new Vector2(10f, 0.2f));

            Assert.That(_trajectory.ClearTime(Thrown(new Vector2(4f, 0f)), Horizon), Is.EqualTo(Horizon));
        }

        [Test]
        public void BodyOutsideTheWallLayersDoesNotEndTheFlight()
        {
            _world.Space.CreateBody(_world.Lifetime, _world.Box("Character", new Vector2(5.5f, 0f), new Vector2(1f, 30f), Vector2.zero, PhysicsTestWorld.Character), PhysicsBody.BodyType.Static);

            Assert.That(_trajectory.ClearTime(Thrown(new Vector2(10f, 0f)), Horizon), Is.EqualTo(Horizon));
        }

        [Test]
        public void FallingThrowMeetsTheFloor()
        {
            _world.Static("Floor", new Vector2(0f, -3f), new Vector2(40f, 1f));

            var clear = _trajectory.ClearTime(Thrown(Vector2.zero), Horizon);

            var fall = 0.5f + 2.5f;
            Assert.That(clear, Is.EqualTo(Mathf.Sqrt(2f * fall / -_trajectory.Gravity.y)).Within(0.02f));
        }

        private static RigidState Thrown(Vector2 velocity) =>
            new(Vector2.zero, 0f, velocity, 0f);
    }
}
