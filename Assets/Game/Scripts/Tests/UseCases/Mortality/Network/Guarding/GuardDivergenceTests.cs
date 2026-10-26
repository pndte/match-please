using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Guarding;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardDivergenceTests
    {
        private static readonly GuardState Predicted = new(42.5f, true, false, 30f, 4, 2);

        private readonly GuardDivergence _divergence = new();

        [Test]
        public void SameStateHasNotDiverged() =>
            Assert.That(_divergence.Diverged(Predicted, Predicted), Is.False);

        [Test]
        public void AimRoundedByTheWireHasNotDiverged() =>
            Assert.That(_divergence.Diverged(Predicted, new GuardState(42.5f, true, false, 30.004f, 4, 2)), Is.False);

        [TestCase(42f, true, false, 30f, 4, 2)]
        [TestCase(42.5f, false, false, 30f, 4, 2)]
        [TestCase(42.5f, true, true, 30f, 4, 2)]
        [TestCase(42.5f, true, false, 31f, 4, 2)]
        [TestCase(42.5f, true, false, 30f, 0, 2)]
        [TestCase(42.5f, true, false, 30f, 4, 3)]
        public void AnyOtherDifferenceHasDiverged(
            float health,
            bool raised,
            bool broken,
            float aim,
            int ticksSinceBlow,
            int ticksSinceRegen)
        {
            var actual = new GuardState(health, raised, broken, aim, ticksSinceBlow, ticksSinceRegen);

            Assert.That(_divergence.Diverged(Predicted, actual), Is.True);
        }
    }
}
