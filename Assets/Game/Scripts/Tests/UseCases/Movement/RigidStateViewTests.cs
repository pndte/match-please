using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class RigidStateViewTests
    {
        private HiddenObjects _objects;
        private BodyBox _box;
        private RigidStateView _view;

        [SetUp]
        public void SetUp()
        {
            _objects = new HiddenObjects();
            _box = _objects.Create("Crate").AddComponent<BodyBox>();
            _box.transform.position = new Vector3(0f, 0f, -2f);
            _view = new RigidStateView(_box);
        }

        [TearDown]
        public void TearDown() =>
            _objects.Dispose();

        [Test]
        public void PictureTakesTheInterpolatedPoseAndKeepsItsDepth()
        {
            _view.Show(new RigidState(new Vector2(1f, 1f), -40f, Vector2.zero, 0f), new RigidState(new Vector2(3f, 5f), 0f, Vector2.zero, 0f), 0.25f);

            Assert.That((Vector2)_box.transform.position, Is.EqualTo(new Vector2(1.5f, 2f)));
            Assert.That(_box.transform.position.z, Is.EqualTo(-2f));
            Assert.That(Mathf.DeltaAngle(_box.transform.eulerAngles.z, -30f), Is.EqualTo(0f).Within(1e-3f));
        }

        [Test]
        public void AngleTurnsTheShortWayRound()
        {
            _view.Show(new RigidState(Vector2.zero, 170f, Vector2.zero, 0f), new RigidState(Vector2.zero, -170f, Vector2.zero, 0f), 0.5f);

            Assert.That(Mathf.DeltaAngle(_box.transform.eulerAngles.z, 180f), Is.EqualTo(0f).Within(1e-3f));
        }
    }
}
