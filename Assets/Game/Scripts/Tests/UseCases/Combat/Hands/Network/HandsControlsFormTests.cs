using System;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsFormTests
    {
        private WeaponInventoryConfig _config;
        private FakeHands _hands;
        private HandsControlsForm _form;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
            _hands = new FakeHands(new Vector2(2f, 1f));
            _form = new HandsControlsForm(_config, _hands);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void InputCarriesTheAimTheTriggerTheReloadAndTheGuard()
        {
            var controls = new HandsControls(new Vector2(2f, 4f), new TriggerControls(true, 1f, 2f, default), true, DrawControl.Keep);

            var input = _form.InputOf(controls, true);

            Assert.That(input.Weapon.Aim, Is.EqualTo(90f).Within(1e-4f));
            Assert.That(input.Weapon.Pulled, Is.True);
            Assert.That(input.Weapon.ReloadPressed, Is.True);
            Assert.That(input.Weapon.Guarded, Is.True);
        }

        [TestCase(2f, 1f)]
        [TestCase(2.4f, 1f)]
        [TestCase(2f, 1.8f)]
        [TestCase(-3f, 0.5f)]
        public void ShotPassesThroughTheAimPointWhereverTheHandsAre(float x, float y)
        {
            var point = new Vector2(9f, 3f);
            using var range = new ShootingRange();
            var muzzle = new WeaponMuzzle(range.Point("Muzzle", new Vector2(0.3f, 0f)), range.Orbit(0.5f, 0.5f));
            _hands.MoveTo(new Vector2(x, y));

            var aim = _form.InputOf(new HandsControls(point, default, false, DrawControl.Keep), false).Weapon.Aim;

            var ray = muzzle.RayFrom(_hands.Pose, aim);
            var across = Vector3.Cross(ray.Direction, point - ray.Muzzle).z;
            Assert.That(Mathf.Abs(across), Is.LessThan(1e-3f), "the hands lag behind where the shooter is drawn, and the shot still goes to the point under the cursor");
        }

        [Test]
        public void InputWithoutTheStanceIsNotGuarded()
        {
            var controls = new HandsControls(Vector2.zero, new TriggerControls(false, 0f, 0f, default), false, DrawControl.Keep);

            var input = _form.InputOf(controls, false);

            Assert.That(input.Weapon.Pulled, Is.False);
            Assert.That(input.Weapon.ReloadPressed, Is.False);
            Assert.That(input.Weapon.Guarded, Is.False);
        }

        [Test]
        public void EachWireChoiceBecomesItsCoreChoice()
        {
            Assert.That(ChoiceOf(DrawControl.Keep), Is.EqualTo(DrawChoice.Keep));
            Assert.That(ChoiceOf(DrawControl.Last), Is.EqualTo(DrawChoice.Last));
            Assert.That(ChoiceOf(DrawControl.ToSlot(2)), Is.EqualTo(DrawChoice.ToSlot(_config.Slots[2])));
            Assert.That(ChoiceOf(DrawControl.Scrolled(-1)), Is.EqualTo(DrawChoice.Scrolled(-1)));
        }

        [Test]
        public void EachCoreChoiceBecomesItsWireChoice()
        {
            Assert.That(_form.ControlOf(DrawChoice.Keep), Is.EqualTo(DrawControl.Keep));
            Assert.That(_form.ControlOf(DrawChoice.Last), Is.EqualTo(DrawControl.Last));
            Assert.That(_form.ControlOf(DrawChoice.ToSlot(_config.Slots[1])), Is.EqualTo(DrawControl.ToSlot(1)));
            Assert.That(_form.ControlOf(DrawChoice.Scrolled(1)), Is.EqualTo(DrawControl.Scrolled(1)));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SlotChoiceComesBackFromTheWireUnchanged(int slot)
        {
            var choice = DrawChoice.ToSlot(_config.Slots[slot]);

            Assert.That(ChoiceOf(_form.ControlOf(choice)), Is.EqualTo(choice));
        }

        [Test]
        public void ChoiceOfASlotOutsideTheLayoutNeverGoesToTheWire()
        {
            var stranger = DrawChoice.ToSlot(new WeaponSlot { Kinds = new[] { WeaponKind.Gun } });

            Assert.Throws<InvalidOperationException>(() => _form.ControlOf(stranger));
        }

        [Test]
        public void UnknownWireChoiceThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ChoiceOf(new DrawControl((DrawControlKind)9, 0)));
        }

        [Test]
        public void ScrollOfMoreThanOneSlotThrows()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ChoiceOf(new DrawControl(DrawControlKind.Scroll, 2)));
        }

        private DrawChoice ChoiceOf(DrawControl control) =>
            _form.InputOf(new HandsControls(Vector2.zero, default, false, control), false).Choice;
    }
}
