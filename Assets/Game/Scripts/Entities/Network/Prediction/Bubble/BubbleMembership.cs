using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Network.Prediction.Bubble
{
    public sealed class BubbleMembership : IBubbleMembership, IPredictedBy
    {
        public IReadonlyProperty<bool> Inside => _inside;
        IReadonlyProperty<bool> IPredictedBy.Me => _inside;

        private readonly ViewableProperty<bool> _inside = new(false);
        private readonly Transform _transform;
        private readonly IPredictionBubble _bubble;

        public BubbleMembership(
            Lifetime lifetime,
            Transform transform,
            IPredictionBubble bubble,
            IWorldStep worldStep)
        {
            _transform = transform;
            _bubble = bubble;

            worldStep.Stepped(TickPhase.Prepare).Advise(lifetime, _ => Settle());
        }

        private void Settle() =>
            _inside.Value = _bubble.Contains(_transform.position, _inside.Value);
    }
}
