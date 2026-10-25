using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class AimDeflectionRegistrationTests
    {
        [Test]
        public void WeaponsDeflectionIsRegisteredForItsLifetime()
        {
            var registration = new LifetimeDefinition();
            var weapon = new FakeWeapon();
            var deflection = new SteadyAim();
            var deflections = new AimDeflections();

            new AimDeflectionRegistration(registration.Lifetime, weapon, deflection, deflections);

            Assert.That(deflections.ByWeapon[weapon], Is.SameAs(deflection));

            registration.Terminate();

            Assert.That(deflections.ByWeapon.ContainsKey(weapon), Is.False);
        }
    }
}
