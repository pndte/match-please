using Bw.Entities.Network.LagCompensation;
using Bw.Tests.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.LagCompensation
{
    public sealed class LagCompensationConfigExtensionsTests
    {
        [Test]
        public void ShippedDepthIsTwelveTicksAtThirtyHertz()
        {
            Assert.That(new LagCompensationConfig().RewindTicks(new ManualClock(1f / 30f)), Is.EqualTo(12));
        }

        [TestCase(0.4f, 1f / 60f, 24)]
        [TestCase(0.5f, 0.25f, 2)]
        [TestCase(0.625f, 0.25f, 3)]
        [TestCase(1f, 0.125f, 8)]
        [TestCase(0.034f, 1f / 30f, 2)]
        [TestCase(0f, 1f / 30f, 0)]
        public void DepthIsTheRewindTimeInWholeTicksRoundedUp(float seconds, float tickSeconds, int ticks)
        {
            var config = new LagCompensationConfig { MaxRewindSeconds = seconds };

            Assert.That(config.RewindTicks(new ManualClock(tickSeconds)), Is.EqualTo(ticks));
        }
    }
}
