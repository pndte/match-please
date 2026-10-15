using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class SmoothedStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _offset;
        private Vector2 _fromShift;
        private double _since;
        private int _correctedTick;

        public SmoothedStateView(
            Lifetime lifetime,
            IStateView<TState> view,
            IStateCorrections<TState> corrections,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _ticks = ticks;
            _interpolation = interpolation;
            _config = config;

            corrections.Corrected.Advise(lifetime, Smooth);
        }

        public void Show(TState from, TState to, float progress)
        {
            var offset = _offset * RemainingShare();
            _view.Show(from.Shifted(offset + FromShift()), to.Shifted(offset), progress);
        }

        private void Smooth(StateCorrection<TState> correction)
        {
            var jump = correction.Before.Body.Position - correction.After.Body.Position;
            _fromShift = FromShift() - jump;
            _offset = _offset * RemainingShare() + jump;
            _since = _ticks.PredictedViewTick(_interpolation);
            _correctedTick = _ticks.Current;
            if (jump.magnitude > _config.SnapDistance || _offset.magnitude > _config.SnapDistance)
                _offset = Vector2.zero;
        }

        private Vector2 FromShift() =>
            _ticks.Current == _correctedTick ? _fromShift : Vector2.zero;

        private float RemainingShare() =>
            Mathf.SmoothStep(1f, 0f, (float)((_ticks.PredictedViewTick(_interpolation) - _since) * _ticks.Duration / _config.DecaySeconds));
    }
}
