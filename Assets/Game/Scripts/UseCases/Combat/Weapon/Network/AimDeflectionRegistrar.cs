using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class AimDeflectionRegistrar
    {
        public AimDeflectionRegistrar(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyAimDeflection deflection,
            IAimDeflections deflections)
        {
            deflections.ByWeapon.AddLifetimed(lifetime, weapon, deflection);
        }
    }
}
