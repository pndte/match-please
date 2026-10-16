using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class SmoothedRigidStateView : IStateView<RigidState>
    {
        private readonly IStateView<RigidState> _view;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _offset;
        private float _turn;
        private double _since;

        public SmoothedRigidStateView(
            Lifetime lifetime,
            IStateView<RigidState> view,
            IStateCorrections<RigidState> corrections,
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

        public void Show(RigidState from, RigidState to, float progress)
        {
            var share = RemainingShare();
            _view.Show(Shifted(from, share), Shifted(to, share), progress);
        }

        private void Smooth(StateCorrection<RigidState> correction)
        {
            var share = RemainingShare();
            _offset = _offset * share + correction.Before.Position - correction.After.Position;
            _turn = _turn * share + Mathf.DeltaAngle(correction.After.Angle, correction.Before.Angle);
            _since = _ticks.PredictedViewTick(_interpolation);
            if (_offset.magnitude <= _config.SnapDistance)
                return;

            _offset = Vector2.zero;
            _turn = 0f;
        }

        private RigidState Shifted(RigidState state, float share) =>
            new(state.Position + _offset * share, state.Angle + _turn * share, state.Velocity, state.AngularVelocity);

        private float RemainingShare() =>
            Mathf.SmoothStep(1f, 0f, (float)((_ticks.PredictedViewTick(_interpolation) - _since) * _ticks.Duration / _config.DecaySeconds));
    }
}
