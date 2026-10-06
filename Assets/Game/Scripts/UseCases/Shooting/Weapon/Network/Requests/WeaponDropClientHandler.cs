using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponDropClientHandler
    {
        private readonly IWeaponDropRequest _dropRequest;

        public WeaponDropClientHandler(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IWeaponDropRequest dropRequest)
        {
            _dropRequest = dropRequest;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdateDrop()));
        }

        private void UpdateDrop()
        {
            if (Input.GetKeyDown(KeyCode.G)) //TODO: new input system
                _dropRequest.Requested.Fire(JetBrains.Core.Unit.Instance);
        }
    }
}
