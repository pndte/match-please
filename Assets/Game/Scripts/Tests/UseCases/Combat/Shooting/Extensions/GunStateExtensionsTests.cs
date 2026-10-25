using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Extensions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting.Extensions
{
    public sealed class GunStateExtensionsTests
    {
        [Test]
        public void WithoutReloadClearsOnlyTheReload()
        {
            var state = new GunState(45f, 2, 3, 6, 11);

            Assert.That(state.WithoutReload(), Is.EqualTo(new GunState(45f, 2, 3, 0, 11)));
        }

        [Test]
        public void WithoutReloadOfAGunNotReloadingChangesNothing()
        {
            var state = new GunState(45f, 2, 3, 0, 11);

            Assert.That(state.WithoutReload(), Is.EqualTo(state));
        }

        [TestCase(5, 4)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        public void CooledDownTakesOneTickOffThePauseAndStopsAtZero(int cooldown, int cooled)
        {
            var state = new GunState(-90f, 1, cooldown, 4, 3);

            Assert.That(state.CooledDown(), Is.EqualTo(new GunState(-90f, 1, cooled, 4, 3)));
        }
    }
}
