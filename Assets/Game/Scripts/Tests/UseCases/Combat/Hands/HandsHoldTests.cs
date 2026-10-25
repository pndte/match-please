using System;
using Bw.Entities.Extensions;
using Bw.Tests.UseCases.Combat.Hands.Doubles;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Movement.Physics;
using JetBrains.Lifetimes;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Hands
{
    public sealed class HandsHoldTests
    {
        private static readonly Vector2 Held = new(1.5f, 0.5f);

        private CombatPhysics _physics;
        private InventoryRig _inventories;
        private HoldOval _oval;
        private HandsRig _rig;
        private HeldBoxes _boxes;
        private HandsHold _hold;
        private FakeWeapon _fists;
        private FakeWeapon _crate;
        private FakeHeldBox _box;

        [SetUp]
        public void SetUp()
        {
            _physics = new CombatPhysics();
            _inventories = new InventoryRig(_physics.Lifetime);
            _oval = _physics.Objects.Create("holder", new Vector2(10f, 0f), CombatPhysics.Character).AddComponent<HoldOval>();
            _rig = new HandsRig(_physics.Lifetime, _inventories, _oval, space: _physics.Space);
            _boxes = new HeldBoxes();
            _hold = new HandsHold(_physics.Lifetime, _rig.Draw, _boxes, _rig.Hands, _rig.Grip);
            _fists = _inventories.Weapon(WeaponKind.Fists);
            _crate = _inventories.Weapon(WeaponKind.Prop);
            _box = new FakeHeldBox(_physics.Box("Crate", new Vector2(-20f, 0f), Vector2.one, 16), CombatPhysics.HeldProp, Held);
            _rig.Inventory.Give(_inventories.FistsSlot, _fists);
            _rig.Step(DrawChoice.Keep);
        }

        [TearDown]
        public void TearDown()
        {
            _inventories.Dispose();
            _physics.Dispose();
        }

        [Test]
        public void WeaponWithoutAHeldBoxIsNotGripped()
        {
            Assert.That(_hold.Gripped, Is.Empty);
            Assert.That(_physics.ShapesAt(HandsPosition + Held, CombatPhysics.HeldProp), Is.Empty);
        }

        [Test]
        public void TakenBoxIsGrippedWhereItSitsInTheHands()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);

            _rig.Hands.Take(_crate);

            Assert.That(_hold.Gripped, Is.EqualTo(new[] { _crate }));
            Assert.That(_rig.Grip.Pose.Center, Is.EqualTo(HandsPosition + Held));
            Assert.That(_physics.ShapesAt(HandsPosition + Held, CombatPhysics.HeldProp), Has.Length.EqualTo(1));
        }

        [Test]
        public void BoxRegisteredWhileItsWeaponIsInHandsIsGrippedAtOnce()
        {
            _rig.Hands.Take(_crate);

            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);

            Assert.That(_hold.Gripped, Is.EqualTo(new[] { _crate }));
            Assert.That(_rig.Grip.Pose.Center, Is.EqualTo(HandsPosition + Held));
        }

        [Test]
        public void CarryPlacesTheGripAtTheHandsNewPose()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);
            _rig.MoveTo(new Vector2(30f, 2f));

            _hold.Carry();

            Assert.That(_rig.Grip.Pose.Center, Is.EqualTo(new Vector2(30f, 2f) + Held));
            Assert.That(_physics.ShapesAt(new Vector2(30f, 2f) + Held, CombatPhysics.HeldProp), Has.Length.EqualTo(1));
            Assert.That(_physics.ShapesAt(new Vector2(10f, 0f) + Held, CombatPhysics.HeldProp), Is.Empty);
        }

        [Test]
        public void TurnedBoxPlacesTheGripAgain()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);

            _box.Turn(new Vector2(0f, 2f), 90f);

            Assert.That(_rig.Grip.Pose.Center, Is.EqualTo(HandsPosition + new Vector2(0f, 2f)));
            Assert.That(_rig.Grip.Pose.Angle, Is.EqualTo(90f));
        }

        [Test]
        public void EveryPlacementStartsFromTheGripsLastPose()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);

            _hold.Carry();
            _hold.Carry();

            Assert.That(_box.Asked, Has.Count.EqualTo(3));
            Assert.That(_box.Asked[0].Pushed, Is.EqualTo(Vector2.zero));
            Assert.That(_box.Asked[1].Pushed, Is.EqualTo(Vector2.up));
            Assert.That(_box.Asked[2].Pushed, Is.EqualTo(Vector2.up * 2f));
        }

        [Test]
        public void StowedBoxLeavesTheGripAndTheWorld()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);

            _rig.Step(DrawChoice.ToSlot(_inventories.FistsSlot));

            Assert.That(_hold.Gripped, Is.Empty);
            Assert.That(_physics.ShapesAt(HandsPosition + Held, CombatPhysics.HeldProp), Is.Empty);
            Assert.Throws<InvalidOperationException>(() => _rig.Grip.Place(default));
        }

        [Test]
        public void StowedBoxTurningMovesNothing()
        {
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);
            _rig.Step(DrawChoice.ToSlot(_inventories.FistsSlot));
            var asked = _box.Asked.Count;

            _box.Turn(Vector2.zero, 45f);
            _hold.Carry();

            Assert.That(_box.Asked, Has.Count.EqualTo(asked));
        }

        [Test]
        public void UnregisteredBoxLeavesTheGripWhileItsWeaponStaysInHands()
        {
            var registration = new LifetimeDefinition(_physics.Lifetime);
            _boxes.ByWeapon.AddLifetimed(registration.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);

            registration.Terminate();

            Assert.That(_hold.Gripped, Is.Empty);
            Assert.That(_rig.Draw.Drawn, Is.EqualTo(new[] { _crate }));
            Assert.That(_physics.ShapesAt(HandsPosition + Held, CombatPhysics.HeldProp), Is.Empty);
        }

        [Test]
        public void HoldingBodyPlacesTheBodyAndThenWhatTheHandsHold()
        {
            var holding = new HoldingBody(new PhysicsBodyPose(_rig.Body), _hold);
            _boxes.ByWeapon.AddLifetimed(_physics.Lifetime, _crate, _box);
            _rig.Hands.Take(_crate);

            holding.Place(new Vector2(-4f, 6f), 0f);

            Assert.That(_rig.Body.position, Is.EqualTo(new Vector2(-4f, 6f)));
            Assert.That(_rig.Grip.Pose.Center, Is.EqualTo(new Vector2(-4f, 6f) + Held));
        }

        [Test]
        public void HoldingBodyTurnsTheBody()
        {
            var holding = new HoldingBody(new PhysicsBodyPose(_rig.Body), _hold);

            holding.Place(new Vector2(-4f, 6f), 30f);

            Assert.That(_rig.Body.rotation.angle * Mathf.Rad2Deg, Is.EqualTo(30f).Within(1e-3f));
        }

        private Vector2 HandsPosition => _rig.Body.position;
    }
}
