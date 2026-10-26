using Bw.Entities;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class WeaponPickupClientHandler
    {
        private const KeyCode PickupKey = KeyCode.E;

        private readonly IPickupTargetSelector _targetSelector;
        private readonly IWeaponCollections _weapons;
        private readonly INetRequestSender<WeaponPickup> _pickup;

        public WeaponPickupClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IPickupTargetSelector targetSelector,
            IWeaponCollections weapons,
            INetRequestSender<WeaponPickup> pickup)
        {
            _targetSelector = targetSelector;
            _weapons = weapons;
            _pickup = pickup;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdatePickup()));
        }

        private void UpdatePickup()
        {
            if (Input.GetKeyDown(PickupKey)) //TODO: new input system
                _targetSelector.Target.Value.Switch(() => { }, Request);
        }

        private void Request(IFloorWeapon weapon)
        {
            if (_weapons.ByIds.TryGetLeft(weapon.Weapon, out var id))
                _pickup.Send(new WeaponPickup(id));
        }
    }
}
