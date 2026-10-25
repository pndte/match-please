using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class PhysicsBodyPoseTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;
        private PhysicsBodyPose _pose;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic, position = new Vector2(1f, 2f) });
            _pose = new PhysicsBodyPose(_body);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void PlaceMovesTheBody()
        {
            _pose.Place(new Vector2(7f, -1f), 0f);

            Assert.That(_body.position, Is.EqualTo(new Vector2(7f, -1f)));
        }

        [TestCase(25f)]
        [TestCase(-90f)]
        [TestCase(180f)]
        public void PlaceTurnsTheBodyByTheAngleInDegrees(float angle)
        {
            _pose.Place(new Vector2(1f, 2f), angle);

            Assert.That(Mathf.DeltaAngle(_body.rotation.angle * Mathf.Rad2Deg, angle), Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void PlacingAtTheSamePoseKeepsTheBodyMoving()
        {
            _body.linearVelocity = new Vector2(3f, 0f);

            _pose.Place(new Vector2(1f, 2f), 0f);

            Assert.That(_body.linearVelocity, Is.EqualTo(new Vector2(3f, 0f)));
        }
    }
}
