using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.Network
{
    public sealed class TimelineBlendView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _shown;
        private Vector2 _offset;
        private bool _switched;
        private bool _shownBefore;

        public TimelineBlendView(
            Lifetime lifetime,
            IStateView<TState> view,
            IPredictedBy predictedBy,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
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

            var offset = _offset * _decay.Remaining;
            _shown = position + offset;
            _view.Show(from.Shifted(offset), to.Shifted(offset), progress);
        }

        private void Blend(Vector2 jump)
        {
            _offset = jump.magnitude > _config.SnapDistance ? Vector2.zero : jump;
            _decay.Restart();
        }
    }
}
