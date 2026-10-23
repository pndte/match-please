using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponControlRegistration
    {
        public WeaponControlRegistration(
            Lifetime lifetime,
            IWeapon weapon,
            IControlledBy controlledBy,
            IWeaponControlledBy registry)
        {
            registry.ByWeapon.AddLifetimed(lifetime, weapon, controlledBy);
        }
    }
}
