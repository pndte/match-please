using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting.Extensions
{
    public sealed class PelletsConfigExtensionsTests
    {
        [TestCase(1, 14f, 0, 0f)]
        [TestCase(1, 0f, 0, 0f)]
        [TestCase(2, 10f, 0, -5f)]
        [TestCase(2, 10f, 1, 5f)]
        [TestCase(3, 6f, 1, 0f)]
        [TestCase(7, 14f, 0, -7f)]
        [TestCase(7, 14f, 1, -14f / 3f)]
        [TestCase(7, 14f, 3, 0f)]
        [TestCase(7, 14f, 6, 7f)]
        [TestCase(4, 0f, 2, 0f)]
        public void PelletsFanEvenlyAcrossTheCone(
            int count,
            float cone,
            int pellet,
            float angle)
        {
            var pellets = new PelletsConfig { Count = count, Cone = cone };

            Assert.That(pellets.AngleOf(pellet), Is.EqualTo(angle).Within(1e-4f));
        }

        [Test]
        public void FanIsSymmetricAroundTheAim()
        {
            var pellets = new PelletsConfig { Count = 6, Cone = 20f };

            for (var pellet = 0; pellet < pellets.Count; pellet++)
                Assert.That(pellets.AngleOf(pellet), Is.EqualTo(-pellets.AngleOf(pellets.Count - 1 - pellet)).Within(1e-4f));
        }
    }
}
