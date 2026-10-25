using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Network.Prediction;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class TimelineBlendView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IMovementCollider _collider;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _shown;
        private Vector2 _offset;
        private bool _switched;
        private bool _shownBefore;

        public TimelineBlendView( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IStateView<TState> view,
            IPredictedBy predictedBy,
            IMovementCollider collider,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _collider = collider;
            _decay = new CorrectionDecay(ticks, interpolation, config.SwitchBlendSeconds);
            _config = config;

            predictedBy.Me.Advise(lifetime, _ => _switched = true);
        }

        public void Show(TState from, TState to, float progress)
        {
            var position = Vector2.Lerp(from.Body.Position, to.Body.Position, progress);
            if (_switched && _shownBefore)
                Blend(_shown - position);

            _switched = false;
            _shownBefore = true;

            var wanted = _offset * _decay.Remaining;
            var offset = _collider.ReachableOffset(position, wanted);
            var drift = (_offset * _decay.Rate).ReachableDrift(wanted, offset);
            _shown = position + offset;
            _view.Show(from.Shifted(offset).Drifted(drift), to.Shifted(offset).Drifted(drift), progress);
        }

        private void Blend(Vector2 jump)
        {
            _offset = jump.magnitude > _config.SnapDistance ? Vector2.zero : jump;
            _decay.Restart();
        }
    }
}
