using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class SmoothedStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _offset;

        public SmoothedStateView(
            Lifetime lifetime,
            IStateView<TState> view,
            IStateCorrections<TState> corrections,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _decay = new CorrectionDecay(ticks, interpolation, config.DecaySeconds);
            _config = config;

            corrections.Corrected.Advise(lifetime, Smooth);
        }

        public void Show(TState from, TState to, float progress)
        {
            var offset = _offset * _decay.Remaining;
            _view.Show(from.Shifted(offset), to.Shifted(offset), progress);
        }

        private void Smooth(StateCorrection<TState> correction)
        {
            var jump = correction.Before.Body.Position - correction.After.Body.Position;
            _offset = _offset * _decay.Remaining + jump;
            _decay.Restart();
            if (jump.magnitude > _config.SnapDistance || _offset.magnitude > _config.SnapDistance)
                _offset = Vector2.zero;
        }
    }
}
