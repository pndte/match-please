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

        private readonly IPickupTargeting _targeting;
        private readonly INetRequestSender<WeaponPickup> _pickup;

        public WeaponPickupClientHandler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            IPickupTargeting targeting,
            INetRequestSender<WeaponPickup> pickup)
        {
            _targeting = targeting;
            _pickup = pickup;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdatePickup()));
        }

        private void UpdatePickup()
        {
            if (Input.GetKeyDown(PickupKey)) //TODO: new input system
                _targeting.Target.Value.Switch(() => { }, weapon => _pickup.Send(new WeaponPickup(weapon.Position)));
        }
    }
}
