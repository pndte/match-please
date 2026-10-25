using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class RigidStateBodyExtensionsTests
    {
        private PhysicsTestWorld _physics;
        private PhysicsBody _body;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _body = _physics.Dynamic("Crate", Vector2.zero, Vector2.one);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void StateReadsThePoseInDegreesAndTheVelocities()
        {
            _body.transform = new PhysicsTransform(new Vector2(2f, 3f), new PhysicsRotate(30f * Mathf.Deg2Rad));
            _body.linearVelocity = new Vector2(4f, -5f);
            _body.angularVelocity = 90f;

            var state = _body.State();

            Assert.That(state.Position, Is.EqualTo(new Vector2(2f, 3f)));
            Assert.That(state.Angle, Is.EqualTo(30f).Within(1e-3f));
            Assert.That(state.Velocity, Is.EqualTo(new Vector2(4f, -5f)));
            Assert.That(state.AngularVelocity, Is.EqualTo(90f).Within(1e-3f));
        }

        [Test]
        public void AppliedStateIsReadBack()
        {
            var applied = new RigidState(new Vector2(-1f, 7f), -45f, new Vector2(0.5f, 2f), -180f);

            _body.ApplyRigidState(applied);

            var state = _body.State();
            Assert.That(state.Position.x, Is.EqualTo(-1f).Within(1e-5f));
            Assert.That(state.Position.y, Is.EqualTo(7f).Within(1e-5f));
            Assert.That(state.Angle, Is.EqualTo(-45f).Within(1e-3f));
            Assert.That(state.Velocity, Is.EqualTo(new Vector2(0.5f, 2f)));
            Assert.That(state.AngularVelocity, Is.EqualTo(-180f).Within(1e-3f));
        }

        [Test]
        public void AppliedSpinTurnsTheBodyByDegreesPerSecond()
        {
            _body.gravityScale = 0f;
            _body.ApplyRigidState(new RigidState(Vector2.zero, 0f, Vector2.zero, 90f));

            _physics.Step(3);

            Assert.That(_body.State().Angle, Is.EqualTo(90f * 3f * _physics.Clock.Duration).Within(0.01f));
        }
    }
}
