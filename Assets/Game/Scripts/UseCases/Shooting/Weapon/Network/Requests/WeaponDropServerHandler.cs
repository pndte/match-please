using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandler
    {
        private readonly IWeapon _weapon;
        private readonly IWeaponInventories _inventories;

        public WeaponDropServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            INetRequestReceiver<Unit> drop,
            IWeaponInventories inventories)
        {
            _weapon = weapon;
            _inventories = inventories;

            drop.Received.Advise(lifetime, _ => Drop());
        }

        private void Drop()
        {
            if (_inventories.TryFindInventoryCarrying(_weapon, out var inventory) && inventory.InHands(_weapon))
                inventory.DropAndDrawNext(_weapon);
        }
    }
}
