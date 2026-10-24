using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class ReloadSampler
    {
        private bool _pressed;

        public ReloadSampler(Lifetime lifetime, IReadonlyControlledBy controlledBy)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => LatchPress());
                controlledLifetime.OnTermination(() => _pressed = false);
            });
        }

        public bool Sample()
        {
            var pressed = _pressed;
            _pressed = false;
            return pressed;
        }

        private void LatchPress()
        {
            if (Input.GetKeyDown(KeyCode.R)) //TODO: new input system
                _pressed = true;
        }
    }
}
