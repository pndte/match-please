using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Props.View.Held
{
    public sealed class HeldPropView
    {
        private readonly Transform _prop;
        private readonly IHeldPropViewModel _held;

        public HeldPropView(
            Lifetime lifetime,
            Transform prop,
            IWeaponCarryViewModel carry,
            IHeldPropViewModel held)
        {
            _prop = prop;
            _held = held;

            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
                Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate, drawnLifetime).Subscribe(_ => Place()));
        }

        private void Place()
        {
            var position = _held.Position;
            _prop.SetPositionAndRotation(new Vector3(position.x, position.y, _prop.position.z), Quaternion.Euler(0f, 0f, _held.Angle));
        }
    }
}
