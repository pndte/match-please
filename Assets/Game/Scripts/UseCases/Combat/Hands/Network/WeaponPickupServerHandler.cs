using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class WeaponPickupServerHandler
    {
        private readonly IWeaponCollections _weapons;
        private readonly PickableWeapons _pickables;
        private readonly IHands _hands;

        public WeaponPickupServerHandler(
            Lifetime lifetime,
            IReadonlyMortal character,
            INetRequestReceiver<WeaponPickup> pickup,
            IWeaponCollections weapons,
            PickableWeapons pickables,
            IHands hands)
        {
            _weapons = weapons;
            _pickables = pickables;
            _hands = hands;

            character.State.WhenAlive(lifetime, aliveLifetime =>
                pickup.Received.Advise(aliveLifetime, request => PickUp(request.Weapon)));
        }

        private void PickUp(ulong id)
        {
            if (_weapons.ByIds.TryGetValue(id, out var weapon) && _pickables.Pickable(weapon) && _hands.FreeFor(weapon))
                _hands.Take(weapon);
        }
    }
}
