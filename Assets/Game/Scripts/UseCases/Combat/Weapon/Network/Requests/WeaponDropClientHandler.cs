using Bw.Entities;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class WeaponDropClientHandler
    {
        private readonly INetRequestSender<JetBrains.Core.Unit> _drop;

        public WeaponDropClientHandler(
            Lifetime lifetime,
            IControlledBy controlledBy,
            INetRequestSender<JetBrains.Core.Unit> drop)
        {
            _drop = drop;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => UpdateDrop()));
        }

        private void UpdateDrop()
        {
            if (Input.GetKeyDown(KeyCode.G)) //TODO: new input system
                _drop.Send(JetBrains.Core.Unit.Instance);
        }
    }
}
