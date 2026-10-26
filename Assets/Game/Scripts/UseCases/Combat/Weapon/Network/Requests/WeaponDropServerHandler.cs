using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class WeaponDropServerHandler
    {
        private readonly IWeapon _weapon;
        private readonly IReadonlyWeaponsInHands _inHands;
        private readonly IReadonlyWeaponCarriers _carriers;

        public WeaponDropServerHandler(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponsInHands inHands,
            INetRequestReceiver<Unit> drop,
            IReadonlyWeaponCarriers carriers)
        {
            _weapon = weapon;
            _inHands = inHands;
            _carriers = carriers;

            drop.Received.Advise(lifetime, _ => Drop());
        }

        private void Drop()
        {
            if (_inHands.ByWeapon.ContainsKey(_weapon) && _carriers.ByWeapon.TryGetValue(_weapon, out var carrier))
                carrier.Inventory.Drop(_weapon);
        }
    }
}
