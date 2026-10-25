using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class BodyPoseViewTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;
        private PlacedBody _picture;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.Space.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic });
            _picture = new PlacedBody();
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void BodyAndPictureGoToTheSamePlaceBetweenTheStates()
        {
            var view = new BodyPoseView<MovementState>(new PhysicsBodyPose(_body), _picture);

            view.Show(At(new Vector2(0f, 0f)), At(new Vector2(2f, -4f)), 0.75f);

            Assert.That(_body.position, Is.EqualTo(new Vector2(1.5f, -3f)));
            Assert.That(_picture.Placed, Is.EqualTo(new[] { new Vector2(1.5f, -3f) }));
        }

        private static MovementState At(Vector2 position) =>
            new(new BodyState(position, Vector2.zero, 0f, true), default);
    }
}
