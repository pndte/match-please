using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Guarding.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Guarding.Extensions
{
    public sealed class GuardStateExtensionsTests
    {
        [TestCase(true)]
        [TestCase(false)]
        public void WithRaisedChangesOnlyTheStance(bool broken)
        {
            var state = new GuardState(30f, false, broken).WithRaised(true);

            Assert.That(state, Is.EqualTo(new GuardState(30f, true, broken)));
        }

        [TestCase(100f, 30f, 70f)]
        [TestCase(30f, 30f, 0f)]
        [TestCase(10f, 25f, 0f)]
        [TestCase(10f, 0f, 10f)]
        public void BlowTakesItsDamageDownToZero(float health, float damage, float left)
        {
            var state = new GuardState(health, true, false).AfterBlow(damage);

            Assert.That(state, Is.EqualTo(new GuardState(left, true, false)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void BrokenGuardIsEmptyAndKeepsItsStance(bool raised)
        {
            var state = new GuardState(5f, raised, false).AsBroken();

            Assert.That(state, Is.EqualTo(new GuardState(0f, raised, true)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MendedGuardIsWholeAgainAtTheGivenHealthAndKeepsItsStance(bool raised)
        {
            var state = new GuardState(0f, raised, true).Mended(8f);

            Assert.That(state, Is.EqualTo(new GuardState(8f, raised, false)));
        }

        [TestCase(50f, 8f, 58f)]
        [TestCase(95f, 8f, 100f)]
        [TestCase(100f, 8f, 100f)]
        public void RegenAddsItsAmountUpToTheMaximum(float health, float amount, float healed)
        {
            var state = new GuardState(health, false, false).AfterRegen(amount, 100f);

            Assert.That(state, Is.EqualTo(new GuardState(healed, false, false)));
        }
    }
}
