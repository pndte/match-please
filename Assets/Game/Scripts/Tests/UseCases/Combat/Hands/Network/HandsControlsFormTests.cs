using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
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
        private HandsControlsForm _form;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
            _form = new HandsControlsForm(_config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void InputCarriesTheAimTheTriggerTheReloadAndTheGuard()
        {
            var controls = new HandsControls(30f, new TriggerControls(true, 1f, 2f, default), true, DrawControl.Keep);

            var input = _form.InputOf(controls, true);

            Assert.That(input.Weapon.Aim, Is.EqualTo(30f));
            Assert.That(input.Weapon.Pulled, Is.True);
            Assert.That(input.Weapon.ReloadPressed, Is.True);
            Assert.That(input.Weapon.Guarded, Is.True);
        }

        [Test]
        public void InputWithoutTheStanceIsNotGuarded()
        {
            var controls = new HandsControls(0f, new TriggerControls(false, 0f, 0f, default), false, DrawControl.Keep);

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
            _form.InputOf(new HandsControls(0f, default, false, control), false).Choice;
    }
}
