using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Movement.Network.Prediction;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class SmoothedAimView<TState> : IStateView<TState> where TState : struct, IWeaponState, IAimedState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly CorrectionDecay _decay;

        private float _offset;

        public SmoothedAimView(
            Lifetime lifetime,
            IStateView<TState> view,
            IStateCorrections<TState> corrections,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            CorrectionSmoothingConfig config)
        {
            _view = view;
            _decay = new CorrectionDecay(ticks, interpolation, config.DecaySeconds);

            corrections.Corrected.Advise(lifetime, Smooth);
        }

        public void Show(TState from, TState to, float progress)
        {
            var offset = _offset * _decay.Remaining;
            _view.Show(from.Turned(offset), to.Turned(offset), progress);
        }

        private void Smooth(StateCorrection<TState> correction)
        {
            _offset = _offset * _decay.Remaining + Mathf.DeltaAngle(correction.After.Aim, correction.Before.Aim);
            _decay.Restart();
        }
    }
}
