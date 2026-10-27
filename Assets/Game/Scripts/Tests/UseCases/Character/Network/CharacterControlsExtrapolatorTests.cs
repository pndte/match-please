using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterControlsExtrapolatorTests
    {
        private readonly CharacterControlsExtrapolator _extrapolator = new();

        [Test]
        public void GuessKeepsTheRunAndTheHeldJump()
        {
            var last = new CharacterControls(new MovementControls(-0.5f, true, true, true), Controls.Firing, GuardStance.Lowered);

            var guessed = _extrapolator.Continued(last, 2).Movement;

            Assert.That(guessed.Horizontal, Is.EqualTo(-0.5f));
            Assert.That(guessed.JumpHeld, Is.True);
        }

        [Test]
        public void GuessReleasesTheJumpPressAndDown()
        {
            var last = new CharacterControls(new MovementControls(1f, true, true, true), Controls.Firing, GuardStance.Lowered);

            var guessed = _extrapolator.Continued(last, 1).Movement;

            Assert.That(guessed.JumpPressed, Is.False);
            Assert.That(guessed.Down, Is.False);
        }

        [Test]
        public void HeldTriggerStaysHeldWithItsDelays()
        {
            var last = Controls.RunningAndFiring;

            var trigger = _extrapolator.Continued(last, 2).Hands.Trigger;

            Assert.That(trigger.Pulled, Is.True);
            Assert.That(trigger.ViewDelay, Is.EqualTo(2.5f));
            Assert.That(trigger.PredictedDelay, Is.EqualTo(1.5f));
        }

        [Test]
        public void FreshPressIsGuessedReleased()
        {
            var fresh = new TriggerPress(5, 0, 1f, 1f, 0);
            var last = new CharacterControls(Controls.Running, Hands(new TriggerControls(true, 1f, 1f, fresh)), GuardStance.Lowered);

            var trigger = _extrapolator.Continued(last, 1).Hands.Trigger;

            Assert.That(trigger.Pulled, Is.False);
        }

        [TestCase(0, 3)]
        [TestCase(1, 4)]
        [TestCase(5, 8)]
        [TestCase(300, 255)]
        public void LastPressAgesByTheMissingTicks(int ticks, int age)
        {
            var last = Controls.RunningAndFiring;

            var press = _extrapolator.Continued(last, ticks).Hands.Trigger.Press;

            Assert.That(press.Age, Is.EqualTo(age));
            Assert.That(press.Number, Is.EqualTo(Controls.Press.Number));
            Assert.That(press.ViewDelay, Is.EqualTo(Controls.Press.ViewDelay));
        }

        [Test]
        public void ReleasedTriggerKeepsAgingItsLastPress()
        {
            var released = new CharacterControls(Controls.Running, Hands(new TriggerControls(false, 0f, 0f, Controls.Press)), GuardStance.Lowered);

            var trigger = _extrapolator.Continued(released, 2).Hands.Trigger;

            Assert.That(trigger.Pulled, Is.False);
            Assert.That(trigger.Press.Age, Is.EqualTo(Controls.Press.Age + 2));
        }

        [Test]
        public void ReloadAndWeaponChoiceLiveOneTick()
        {
            var last = new CharacterControls(Controls.Running, new HandsControls(Vector2.one, default, true, DrawControl.ToSlot(1)), GuardStance.Lowered);

            var hands = _extrapolator.Continued(last, 1).Hands;

            Assert.That(hands.ReloadPressed, Is.False);
            Assert.That(hands.Choice, Is.EqualTo(DrawControl.Keep));
        }

        [Test]
        public void GuessKeepsTheAim()
        {
            var last = Controls.RunningAndFiring;

            Assert.That(_extrapolator.Continued(last, 3).Hands.AimPoint, Is.EqualTo(Controls.Firing.AimPoint));
        }

        [TestCase(true, 135f)]
        [TestCase(false, 0f)]
        public void GuessKeepsTheGuardStance(bool raised, float aim)
        {
            var last = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(raised, aim));

            var guard = _extrapolator.Continued(last, 4).Guard;

            Assert.That(guard.Raised, Is.EqualTo(raised));
            Assert.That(guard.Aim, Is.EqualTo(aim));
        }

        private static HandsControls Hands(TriggerControls trigger) =>
            new(new Vector2(3f, 1.75f), trigger, false, DrawControl.Keep);
    }
}
