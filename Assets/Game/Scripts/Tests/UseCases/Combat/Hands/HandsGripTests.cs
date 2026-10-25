using System;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HandsGripTests
    {
        private CombatPhysics _physics;
        private FakeHeldBox _box;
        private HandsGrip _grip;

        [SetUp]
        public void SetUp()
        {
            _physics = new CombatPhysics();
            _box = new FakeHeldBox(_physics.Box("Crate", new Vector2(50f, 50f), new Vector2(2f, 1f), 16), CombatPhysics.HeldProp, Vector2.zero);
            _grip = new HandsGrip(_physics.Space);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void GripIsNowhereBeforeItEnters()
        {
            Assert.That(_physics.ShapesAt(Vector2.zero, CombatPhysics.HeldProp), Is.Empty);
            Assert.That(_grip.Pose, Is.EqualTo(default(GripPose)));
        }

        [Test]
        public void PlacedGripIsAKinematicBoxOwnedByTheHeldObjectOnItsLayer()
        {
            _grip.Enter(_physics.Lifetime, _box);

            _grip.Place(new GripPose(new Vector2(3f, 4f), 0f, Vector2.zero));

            var shapes = _physics.ShapesAt(new Vector2(3f, 4f), CombatPhysics.HeldProp);
            Assert.That(shapes, Has.Length.EqualTo(1));
            Assert.That(shapes[0].userData.objectValue, Is.SameAs(_box.Box.gameObject));
            Assert.That(shapes[0].Layer(), Is.EqualTo(CombatPhysics.HeldProp));
            Assert.That(shapes[0].body.type, Is.EqualTo(PhysicsBody.BodyType.Kinematic));
        }

        [Test]
        public void GripIsShapedLikeTheHeldBox()
        {
            _grip.Enter(_physics.Lifetime, _box);

            _grip.Place(new GripPose(new Vector2(3f, 4f), 0f, Vector2.zero));

            Assert.That(Covered(new Vector2(3.95f, 4f)), Is.True);
            Assert.That(Covered(new Vector2(4.05f, 4f)), Is.False);
            Assert.That(Covered(new Vector2(3f, 4.45f)), Is.True);
            Assert.That(Covered(new Vector2(3f, 4.55f)), Is.False);
        }

        [Test]
        public void PlacedGripTurnsByThePosesAngle()
        {
            _grip.Enter(_physics.Lifetime, _box);

            _grip.Place(new GripPose(new Vector2(3f, 4f), 90f, Vector2.zero));

            Assert.That(Covered(new Vector2(3.45f, 4f)), Is.True);
            Assert.That(Covered(new Vector2(3.55f, 4f)), Is.False);
            Assert.That(Covered(new Vector2(3f, 4.95f)), Is.True);
            Assert.That(Covered(new Vector2(3f, 5.05f)), Is.False);
        }

        [Test]
        public void PlaceMovesTheGripAndRemembersThePose()
        {
            var pose = new GripPose(new Vector2(-6f, 1f), 0f, new Vector2(0.5f, 0f));
            _grip.Enter(_physics.Lifetime, _box);
            _grip.Place(new GripPose(new Vector2(3f, 4f), 0f, Vector2.zero));

            _grip.Place(pose);

            Assert.That(_grip.Pose, Is.EqualTo(pose));
            Assert.That(_physics.ShapesAt(new Vector2(3f, 4f), CombatPhysics.HeldProp), Is.Empty);
            Assert.That(_physics.ShapesAt(new Vector2(-6f, 1f), CombatPhysics.HeldProp), Has.Length.EqualTo(1));
        }

        [Test]
        public void GripOutsideTheWorldIsNeverPlaced()
        {
            Assert.Throws<InvalidOperationException>(() => _grip.Place(new GripPose(Vector2.zero, 0f, Vector2.zero)));
        }

        [Test]
        public void GripEntersTheWorldOnceAtATime()
        {
            _grip.Enter(_physics.Lifetime, _box);

            Assert.Throws<InvalidOperationException>(() => _grip.Enter(_physics.Lifetime, _box));
        }

        [Test]
        public void EndOfTheHoldTakesTheGripOutOfTheWorld()
        {
            var hold = new LifetimeDefinition();
            _grip.Enter(hold.Lifetime, _box);
            _grip.Place(new GripPose(new Vector2(3f, 4f), 0f, Vector2.zero));

            hold.Terminate();

            Assert.That(_physics.ShapesAt(new Vector2(3f, 4f), CombatPhysics.HeldProp), Is.Empty);
            Assert.Throws<InvalidOperationException>(() => _grip.Place(new GripPose(new Vector2(3f, 4f), 0f, Vector2.zero)));
        }

        [Test]
        public void GripEntersAgainAfterTheHoldEndsAndForgetsTheOldPose()
        {
            var hold = new LifetimeDefinition();
            _grip.Enter(hold.Lifetime, _box);
            _grip.Place(new GripPose(new Vector2(3f, 4f), 30f, Vector2.one));
            hold.Terminate();

            _grip.Enter(_physics.Lifetime, _box);

            Assert.That(_grip.Pose, Is.EqualTo(default(GripPose)));
            Assert.DoesNotThrow(() => _grip.Place(new GripPose(Vector2.zero, 0f, Vector2.zero)));
        }

        private bool Covered(Vector2 point) =>
            _physics.ShapesAt(point, CombatPhysics.HeldProp).Length == 1;
    }
}
