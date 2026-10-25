using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class SmoothedStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IMovementCollider _collider;
        private readonly CorrectionDecay _decay;
        private readonly CorrectionSmoothingConfig _config;

        private Vector2 _offset;

        public SmoothedStateView( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IStateView<TState> view,
            IStateCorrections<TState> corrections,
            IMovementCollider collider,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _collider = collider;
            _decay = new CorrectionDecay(ticks, interpolation, config.DecaySeconds);
            _config = config;

            corrections.Corrected.Advise(lifetime, Smooth);
        }

        public void Show(TState from, TState to, float progress)
        {
            var position = Vector2.Lerp(from.Body.Position, to.Body.Position, progress);
            var wanted = _offset * _decay.Remaining;
            var offset = _collider.ReachableOffset(position, wanted);
            var drift = (_offset * _decay.Rate).ReachableDrift(wanted, offset);
            _view.Show(from.Shifted(offset).Drifted(drift), to.Shifted(offset).Drifted(drift), progress);
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
