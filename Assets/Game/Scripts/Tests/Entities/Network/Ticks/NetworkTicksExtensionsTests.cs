using Bw.Entities.Network.Ticks;
using Bw.Tests.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Ticks
{
    public sealed class NetworkTicksExtensionsTests
    {
        [TestCase(10, 0f, 9d)]
        [TestCase(10, 0.25f, 9.25d)]
        [TestCase(10, 1f, 10d)]
        [TestCase(0, 0.5f, -0.5d)]
        public void PredictedViewIsBetweenThePreviousAndTheCurrentTick(int current, float progress, double viewTick)
        {
            var clock = new ManualClock();
            clock.Tick(current);

            var predicted = clock.PredictedViewTick(new Interpolation(progress));

            Assert.That(predicted, Is.EqualTo(viewTick));
        }

        private sealed class Interpolation : IInterpolationTicks
        {
            public float Progress { get; }
            public double InterpolationTick => 0d;

            public Interpolation(float progress)
            {
                Progress = progress;
            }
        }
    }
}
