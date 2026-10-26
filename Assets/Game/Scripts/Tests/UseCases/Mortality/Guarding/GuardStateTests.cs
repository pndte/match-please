using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality.Guarding;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Guarding
{
    public sealed class GuardStateTests
    {
        [TestCase(true, false, true)]
        [TestCase(true, true, false)]
        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        public void GuardIsUpOnlyWhileRaisedAndWhole(bool raised, bool broken, bool up)
        {
            Assert.That(GuardStateOf.Shown(50f, raised, broken).Up, Is.EqualTo(up));
        }

        [Test]
        public void EqualStatesAreEqualAndHashAlike()
        {
            var state = new GuardState(42.5f, true, false, 30f, 4, 2);
            var same = new GuardState(42.5f, true, false, 30f, 4, 2);

            Assert.That(state.Equals(same), Is.True);
            Assert.That(state.Equals((object)same), Is.True);
            Assert.That(state.GetHashCode(), Is.EqualTo(same.GetHashCode()));
        }

        [TestCase(42f, true, false, 30f, 4, 2)]
        [TestCase(42.5f, false, false, 30f, 4, 2)]
        [TestCase(42.5f, true, true, 30f, 4, 2)]
        [TestCase(42.5f, true, false, 31f, 4, 2)]
        [TestCase(42.5f, true, false, 30f, 5, 2)]
        [TestCase(42.5f, true, false, 30f, 4, 3)]
        public void StatesDifferingInAnyFieldAreUnequal(
            float health,
            bool raised,
            bool broken,
            float aim,
            int ticksSinceBlow,
            int ticksSinceRegen)
        {
            var state = new GuardState(42.5f, true, false, 30f, 4, 2);

            Assert.That(state.Equals(new GuardState(health, raised, broken, aim, ticksSinceBlow, ticksSinceRegen)), Is.False);
        }

        [Test]
        public void StateIsUnequalToAnotherType()
        {
            Assert.That(GuardStateOf.Shown(1f, false, false).Equals("guard"), Is.False);
        }
    }
}
