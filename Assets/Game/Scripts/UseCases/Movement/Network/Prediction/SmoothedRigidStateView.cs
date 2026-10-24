using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class SmoothedRigidStateView : IStateView<RigidState>
    {
        private readonly IStateView<RigidState> _view;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _offset;
        private float _turn;

        public SmoothedRigidStateView(
            Lifetime lifetime,
            IStateView<RigidState> view,
            IStateCorrections<RigidState> corrections,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _decay = new CorrectionDecay(ticks, interpolation, config.DecaySeconds);
            _config = config;

            corrections.Corrected.Advise(lifetime, Smooth);
        }

        public void Show(RigidState from, RigidState to, float progress)
        {
            var share = _decay.Remaining;
            _view.Show(Shifted(from, share), Shifted(to, share), progress);
        }

        private void Smooth(StateCorrection<RigidState> correction)
        {
            var share = _decay.Remaining;
            _offset = _offset * share + correction.Before.Position - correction.After.Position;
            _turn = _turn * share + Mathf.DeltaAngle(correction.After.Angle, correction.Before.Angle);
            _decay.Restart();
            if (_offset.magnitude <= _config.SnapDistance)
                return;

            _offset = Vector2.zero;
            _turn = 0f;
        }

        private RigidState Shifted(RigidState state, float share) =>
            new(state.Position + _offset * share, state.Angle + _turn * share, state.Velocity, state.AngularVelocity);
    }
}
