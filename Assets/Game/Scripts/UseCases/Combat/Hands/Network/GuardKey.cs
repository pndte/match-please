using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class GuardKey
    {
        public IReadonlyProperty<bool> Held => _held;

        private readonly ViewableProperty<bool> _held = new(false);

        public GuardKey(Lifetime lifetime, IReadonlyControlledBy controlledBy)
        {
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                Observable.EveryUpdate(UnityFrameProvider.Update, controlledLifetime).Subscribe(_ => _held.Value = Input.GetKey(KeyCode.Space)); //TODO: new input system
                controlledLifetime.OnTermination(() => _held.Value = false);
            });
        }
    }
}
