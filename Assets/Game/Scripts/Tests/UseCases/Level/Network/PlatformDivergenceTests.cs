using Bw.UseCases.Level;
using Bw.UseCases.Level.Network;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Level.Network
{
    public sealed class PlatformDivergenceTests
    {
        private static readonly PlatformDivergence TenMetres = new(new PlatformRail(Vector2.zero, new Vector2(0f, 10f)));

        [Test]
        public void SameShareDoesNotDiffer()
        {
            Assert.That(TenMetres.Differs(new PlatformState(0.4f), new PlatformState(0.4f)), Is.False);
        }

        [TestCase(0.4f, 0.4009f)]
        [TestCase(0.4f, 0.3991f)]
        public void GapUnderACentimetreOfTravelDoesNotDiffer(float predicted, float actual)
        {
            Assert.That(TenMetres.Differs(new PlatformState(predicted), new PlatformState(actual)), Is.False);
        }

        [TestCase(0.4f, 0.4011f)]
        [TestCase(0.4f, 0.3989f)]
        [TestCase(0f, 1f)]
        public void GapOverACentimetreOfTravelDiffers(float predicted, float actual)
        {
            Assert.That(TenMetres.Differs(new PlatformState(predicted), new PlatformState(actual)), Is.True);
        }

        [Test]
        public void ThresholdIsMeasuredInMetresOfTheRail()
        {
            var oneMetre = new PlatformDivergence(new PlatformRail(Vector2.zero, new Vector2(1f, 0f)));

            Assert.That(oneMetre.Differs(new PlatformState(0.4f), new PlatformState(0.405f)), Is.False);
            Assert.That(TenMetres.Differs(new PlatformState(0.4f), new PlatformState(0.405f)), Is.True);
        }
    }
}
