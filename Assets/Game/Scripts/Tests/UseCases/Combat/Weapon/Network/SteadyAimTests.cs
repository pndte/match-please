using Bw.UseCases.Combat.Weapon.Network;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class SteadyAimTests
    {
        [Test]
        public void SteadyAimIsNeverDeflected()
        {
            Assert.That(new SteadyAim().Current, Is.EqualTo(0f));
        }
    }
}
