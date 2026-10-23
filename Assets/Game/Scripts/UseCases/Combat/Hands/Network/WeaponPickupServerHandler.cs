using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class WeaponPickupServerHandler
    {
        private readonly PickableWeapons _pickables;
        private readonly IHands _hands;

        public WeaponPickupServerHandler(
            Lifetime lifetime,
            IReadonlyMortal character,
            INetRequestReceiver<WeaponPickup> pickup,
            PickableWeapons pickables,
            IHands hands)
        {
            _pickables = pickables;
            _hands = hands;

            character.State.WhenAlive(lifetime, aliveLifetime =>
                pickup.Received.Advise(aliveLifetime, request => PickUp(request.Point)));
        }

        private void PickUp(Vector2 point)
        {
            if (_pickables.TryFindNear(point, out var weapon) && _hands.FreeFor(weapon))
                _hands.Take(weapon);
        }
    }
}
