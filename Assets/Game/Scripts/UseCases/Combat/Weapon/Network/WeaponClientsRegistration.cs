using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponClientsRegistration
    {
        public WeaponClientsRegistration(
            Lifetime lifetime,
            IWeapon weapon,
            IControllingClients clients,
            IWeaponControllingClients registry)
        {
            registry.ByWeapon.AddLifetimed(lifetime, weapon, clients);
        }
    }
}
