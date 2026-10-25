using System.Collections.Generic;
using Bw.Tests.UseCases.Character.Doubles;
using Bw.UseCases.Character.Network;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class CharacterControlsDivergenceTests
    {
        private static readonly CharacterControls Guessed = new(Controls.Running, Controls.Firing, new GuardStance(true, 45f));

        private readonly CharacterControlsDivergence _divergence = new();

        private static IEnumerable<TestCaseData> PlayedDifferently()
        {
            var running = Controls.Running;
            var hands = Controls.Firing;
            var trigger = hands.Trigger;
            var press = trigger.Press;
            var guard = Guessed.Guard;

            yield return Case("Horizontal", new MovementControls(0.5f, running.JumpPressed, running.JumpHeld, running.Down), hands, guard);
            yield return Case("JumpPressed", new MovementControls(running.Horizontal, true, running.JumpHeld, running.Down), hands, guard);
            yield return Case("JumpHeld", new MovementControls(running.Horizontal, running.JumpPressed, false, running.Down), hands, guard);
            yield return Case("Down", new MovementControls(running.Horizontal, running.JumpPressed, running.JumpHeld, true), hands, guard);
            yield return Case("Aim", running, new HandsControls(31f, trigger, hands.ReloadPressed, hands.Choice), guard);
            yield return Case("Pulled", running, Hands(new TriggerControls(false, trigger.ViewDelay, trigger.PredictedDelay, press)), guard);
            yield return Case("PressNumber", running, Hands(Pressed(new TriggerPress(5, press.Age, press.ViewDelay, press.PredictedDelay, press.Late))), guard);
            yield return Case("PressAge", running, Hands(Pressed(new TriggerPress(press.Number, 4, press.ViewDelay, press.PredictedDelay, press.Late))), guard);
            yield return Case("Reload", running, new HandsControls(hands.Aim, trigger, true, hands.Choice), guard);
            yield return Case("Choice", running, new HandsControls(hands.Aim, trigger, hands.ReloadPressed, DrawControl.ToSlot(2)), guard);
            yield return Case("GuardRaised", running, hands, new GuardStance(false, guard.Aim));
            yield return Case("GuardAim", running, hands, new GuardStance(true, 46f));
        }

        private static IEnumerable<TestCaseData> PlayedTheSame()
        {
            var running = Controls.Running;
            var hands = Controls.Firing;
            var trigger = hands.Trigger;
            var press = trigger.Press;

            yield return Case("TriggerViewDelay", running, Hands(new TriggerControls(true, 9f, trigger.PredictedDelay, press)), Guessed.Guard);
            yield return Case("TriggerPredictedDelay", running, Hands(new TriggerControls(true, trigger.ViewDelay, 9f, press)), Guessed.Guard);
            yield return Case("PressViewDelay", running, Hands(Pressed(new TriggerPress(press.Number, press.Age, 9f, press.PredictedDelay, press.Late))), Guessed.Guard);
            yield return Case("PressPredictedDelay", running, Hands(Pressed(new TriggerPress(press.Number, press.Age, press.ViewDelay, 9f, press.Late))), Guessed.Guard);
            yield return Case("PressLate", running, Hands(Pressed(new TriggerPress(press.Number, press.Age, press.ViewDelay, press.PredictedDelay, 3))), Guessed.Guard);
        }

        [Test]
        public void SameControlsDoNotDiverge()
        {
            Assert.That(_divergence.Diverged(Guessed, Guessed), Is.False);
        }

        [TestCaseSource(nameof(PlayedDifferently))]
        public void ControlThePlayerPlayedDifferentlyDiverges(CharacterControls relayed)
        {
            Assert.That(_divergence.Diverged(Guessed, relayed), Is.True);
        }

        [TestCaseSource(nameof(PlayedTheSame))]
        public void TriggerViewDelaysNeverDiverge(CharacterControls relayed)
        {
            Assert.That(_divergence.Diverged(Guessed, relayed), Is.False);
        }

        [Test]
        public void AimOfALoweredGuardNeverDiverges()
        {
            var guessed = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(false, 10f));
            var relayed = new CharacterControls(Controls.Running, Controls.Firing, new GuardStance(false, 170f));

            Assert.That(_divergence.Diverged(guessed, relayed), Is.False);
        }

        private static TestCaseData Case(
            string name,
            MovementControls movement,
            HandsControls hands,
            GuardStance guard) =>
            new TestCaseData(new CharacterControls(movement, hands, guard)).SetName("{m}(" + name + ")");

        private static HandsControls Hands(TriggerControls trigger)
        {
            var hands = Controls.Firing;
            return new HandsControls(hands.Aim, trigger, hands.ReloadPressed, hands.Choice);
        }

        private static TriggerControls Pressed(TriggerPress press)
        {
            var trigger = Controls.Firing.Trigger;
            return new TriggerControls(trigger.Pulled, trigger.ViewDelay, trigger.PredictedDelay, press);
        }
    }
}
