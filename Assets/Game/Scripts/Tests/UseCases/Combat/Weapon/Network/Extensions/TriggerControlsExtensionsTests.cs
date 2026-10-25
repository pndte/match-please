using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Extensions
{
    public sealed class TriggerControlsExtensionsTests
    {
        private static readonly TriggerPress Press = new(4, 2, 1.5f, 0.5f, 1);

        [TestCase(true, 0, false)]
        [TestCase(true, 1, true)]
        [TestCase(true, 7, true)]
        [TestCase(false, 0, false)]
        [TestCase(false, 7, false)]
        public void TriggerWithoutAFreshPressStaysPulledOnlyForAnOlderPress(bool pulled, int age, bool stillPulled)
        {
            var trigger = new TriggerControls(pulled, 3f, 1f, new TriggerPress(1, (byte)age, 0f, 0f, 0));

            Assert.That(trigger.WithoutFreshPress().Pulled, Is.EqualTo(stillPulled));
        }

        [Test]
        public void TriggerWithoutAFreshPressKeepsItsDelaysAndPress()
        {
            var trigger = new TriggerControls(true, 3f, 1f, Press);

            var without = trigger.WithoutFreshPress();

            Assert.That(without.ViewDelay, Is.EqualTo(3f));
            Assert.That(without.PredictedDelay, Is.EqualTo(1f));
            Assert.That(without.Press, Is.EqualTo(Press));
        }

        [Test]
        public void PressedTriggerIsPulledWithThePressDelaysMovedByItsLateness()
        {
            var trigger = new TriggerControls(false, 9f, 9f, default);

            var pressed = trigger.Pressed(Press, 3);

            Assert.That(pressed.Pulled, Is.True);
            Assert.That(pressed.ViewDelay, Is.EqualTo(4.5f));
            Assert.That(pressed.PredictedDelay, Is.EqualTo(3.5f));
            Assert.That(pressed.Press, Is.EqualTo(new TriggerPress(4, 0, 1.5f, 0.5f, 3)));
        }

        [Test]
        public void PressReplacedKeepsThePullAndItsDelays()
        {
            var trigger = new TriggerControls(true, 9f, 8f, default);

            var replaced = trigger.WithPress(Press);

            Assert.That(replaced.Pulled, Is.True);
            Assert.That(replaced.ViewDelay, Is.EqualTo(9f));
            Assert.That(replaced.PredictedDelay, Is.EqualTo(8f));
            Assert.That(replaced.Press, Is.EqualTo(Press));
        }

        [Test]
        public void TriggerWithFiniteDelaysIsFinite()
        {
            Assert.That(new TriggerControls(true, 3f, 1f, Press).IsFinite(), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void AnyDelayThatIsNotANumberMakesTheTriggerInfinite(int broken)
        {
            var delays = new[] { 3f, 1f, 1.5f, 0.5f };
            delays[broken] = float.NaN;
            var trigger = new TriggerControls(true, delays[0], delays[1], new TriggerPress(1, 0, delays[2], delays[3], 0));

            Assert.That(trigger.IsFinite(), Is.False);
        }

        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void InfiniteDelayMakesTheTriggerInfinite(float delay)
        {
            Assert.That(new TriggerControls(true, delay, 1f, Press).IsFinite(), Is.False);
        }
    }
}
