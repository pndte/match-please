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
            Assert.That(new GuardState(50f, raised, broken).Up, Is.EqualTo(up));
        }

        [Test]
        public void EqualStatesAreEqualAndHashAlike()
        {
            var state = new GuardState(42.5f, true, false);
            var same = new GuardState(42.5f, true, false);

            Assert.That(state.Equals(same), Is.True);
            Assert.That(state.Equals((object)same), Is.True);
            Assert.That(state.GetHashCode(), Is.EqualTo(same.GetHashCode()));
        }

        [TestCase(42f, true, false)]
        [TestCase(42.5f, false, false)]
        [TestCase(42.5f, true, true)]
        public void StatesDifferingInAnyFieldAreUnequal(float health, bool raised, bool broken)
        {
            var state = new GuardState(42.5f, true, false);

            Assert.That(state.Equals(new GuardState(health, raised, broken)), Is.False);
        }

        [Test]
        public void StateIsUnequalToAnotherType()
        {
            Assert.That(new GuardState(1f, false, false).Equals("guard"), Is.False);
        }
    }
}
