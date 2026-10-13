using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDrawServerHandler
    {
        private readonly IWeapon _weapon;
        private readonly IWeaponInventories _inventories;

        public WeaponDrawServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            INetRequestReceiver<Unit> draw,
            IWeaponInventories inventories)
        {
            _weapon = weapon;
            _inventories = inventories;

            draw.Received.Advise(lifetime, _ => Draw());
        }

        private void Draw()
        {
            if (_inventories.TryFindInventoryCarrying(_weapon, out var inventory) && inventory.Drawable(_weapon))
                inventory.SwitchTo(_weapon);
        }
    }
}
