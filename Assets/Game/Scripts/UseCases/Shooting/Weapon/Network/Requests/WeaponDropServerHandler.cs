using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandler
    {
        public WeaponDropServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            INetRequestReceiver<Unit> drop,
            IWeaponInventories inventories)
        {
            drop.Received.Advise(lifetime, _ => inventories.Drop(weapon));
        }
    }
}
