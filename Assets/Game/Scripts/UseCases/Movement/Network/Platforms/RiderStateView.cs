using System;
using Bw.Entities;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Platforms
{
    public sealed class RiderStateView<TState> : IStateView<TState> where TState : struct, IPlacedState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IControlledBy _controlledBy;
        private readonly IReplicatedStates<TState> _replicated;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly IPlatformRider _rider;

        public RiderStateView(
            IStateView<TState> view,
            IControlledBy controlledBy,
            IReplicatedStates<TState> replicated,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            IPlatformRider rider)
        {
            _view = view;
            _controlledBy = controlledBy;
            _replicated = replicated;
            _ticks = ticks;
            _interpolation = interpolation;
            _rider = rider;
        }

        public void Show(TState from, TState to, float progress) //TODO: чужой, который сходит с едущей платформы или встаёт на неё, перескакивает на её путь за задержку (до скорость × ~0.1 с): правило одно с откатом на сервере, поэтому без сглаживания — сгладить только картинку, не трогая коллайдер
        {
            if (_controlledBy.Me.Value)
            {
                _view.Show(from, to, progress);
                return;
            }

            var duration = (double)_ticks.Duration;
            var shown = Vector2.Lerp(from.Position, to.Position, progress);
            var angle = Mathf.LerpAngle(from.Angle, to.Angle, progress);
            var shift = _rider.RideShift(shown, angle, ShownTick() * duration, _ticks.PredictedViewTick(_interpolation) * duration);
            _view.Show(from.Shifted(shift), to.Shifted(shift), progress);
        }

        private double ShownTick()
        {
            if (!_replicated.TryGetAt(int.MaxValue, out var newest))
                throw new InvalidOperationException("A remote rider is shown only from the states the server sent, and none has arrived.");

            return Math.Min(_interpolation.InterpolationTick, newest.Tick);
        }
    }
}
