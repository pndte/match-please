using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Guarding.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Guarding.Extensions
{
    public sealed class GuardStateExtensionsTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void FacedTakesOnlyTheStance(bool broken)
        {
            var state = new GuardState(30f, false, broken, 10f, 4, 2).Faced(new GuardStance(true, 135f));

            Assert.That(state, Is.EqualTo(new GuardState(30f, true, broken, 135f, 4, 2)));
        }

        [Test]
        public void CountedAddsATickToBothCounters()
        {
            var state = new GuardState(30f, true, false, 10f, 4, 2).Counted(8);

            Assert.That(state, Is.EqualTo(new GuardState(30f, true, false, 10f, 5, 3)));
        }

        [Test]
        public void CountersRestAtTheRestTicks()
        {
            var state = new GuardState(30f, true, false, 10f, 8, 7).Counted(8);

            Assert.That(state, Is.EqualTo(new GuardState(30f, true, false, 10f, 8, 8)));
        }

        [TestCase(100f, 30f, 70f)]
        [TestCase(30f, 30f, 0f)]
        [TestCase(10f, 25f, 0f)]
        [TestCase(10f, 0f, 10f)]
        public void BlowTakesItsDamageDownToZeroAndRestartsItsCounter(float health, float damage, float left)
        {
            var state = new GuardState(health, true, false, 10f, 6, 3).AfterBlow(damage);

            Assert.That(state, Is.EqualTo(new GuardState(left, true, false, 10f, 0, 3)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrokenGuardIsEmptyAndKeepsItsStance(bool raised)
        {
            var state = new GuardState(5f, raised, false, 10f, 1, 3).AsBroken();

            Assert.That(state, Is.EqualTo(new GuardState(0f, raised, true, 10f, 1, 3)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MendedGuardIsWholeAgainAtTheGivenHealthAndRestartsItsRegen(bool raised)
        {
            var state = new GuardState(0f, raised, true, 10f, 8, 8).Mended(8f);

            Assert.That(state, Is.EqualTo(new GuardState(8f, raised, false, 10f, 8, 0)));
        }

        [TestCase(50f, 8f, 58f)]
        [TestCase(95f, 8f, 100f)]
        public void RegenAddsItsAmountUpToTheMaximumAndRestartsItsCounter(float health, float amount, float healed)
        {
            var state = new GuardState(health, false, false, 0f, 8, 6).AfterRegen(amount, 100f);

            Assert.That(state, Is.EqualTo(new GuardState(healed, false, false, 0f, 8, 0)));
        }
    }
}
