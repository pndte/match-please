using Bw.Entities.Physics;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class RigidPoseViewTests
    {
        private PhysicsTestWorld _physics;
        private BodyBox _box;
        private PhysicsBody _body;
        private RigidPoseView _view;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _box = _physics.Box("Crate", Vector2.zero, Vector2.one);
            _box.transform.position = new Vector3(0f, 0f, 3f);
            _body = _physics.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Kinematic });
            _view = new RigidPoseView(_box, _body);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void PictureAndBodyTakeTheInterpolatedPose()
        {
            _view.Show(new RigidState(Vector2.zero, 0f, Vector2.zero, 0f), new RigidState(new Vector2(4f, 2f), 60f, Vector2.zero, 0f), 0.5f);

            Assert.That((Vector2)_box.transform.position, Is.EqualTo(new Vector2(2f, 1f)));
            Assert.That(_box.transform.position.z, Is.EqualTo(3f));
            Assert.That(_box.transform.eulerAngles.z, Is.EqualTo(30f).Within(1e-3f));
            Assert.That(_body.position, Is.EqualTo(new Vector2(2f, 1f)));
            Assert.That(_body.rotation.angle * Mathf.Rad2Deg, Is.EqualTo(30f).Within(1e-3f));
        }

        [Test]
        public void AngleTurnsTheShortWayRound()
        {
            _view.Show(new RigidState(Vector2.zero, 350f, Vector2.zero, 0f), new RigidState(Vector2.zero, 10f, Vector2.zero, 0f), 0.5f);

            Assert.That(Mathf.DeltaAngle(_box.transform.eulerAngles.z, 0f), Is.EqualTo(0f).Within(1e-3f));
            Assert.That(Mathf.DeltaAngle(_body.rotation.angle * Mathf.Rad2Deg, 0f), Is.EqualTo(0f).Within(1e-3f));
        }
    }
}
