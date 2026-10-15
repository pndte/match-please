using Bw.Entities;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Platforms
{
    public sealed class RiderStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IControlledBy _controlledBy;
        private readonly INetworkTicks _ticks;
        private readonly IInterpolationTicks _interpolation;
        private readonly IMovementCollider _collider;

        public RiderStateView(
            IStateView<TState> view,
            IControlledBy controlledBy,
            INetworkTicks ticks,
            IInterpolationTicks interpolation,
            IMovementCollider collider)
        {
            _view = view;
            _controlledBy = controlledBy;
            _ticks = ticks;
            _interpolation = interpolation;
            _collider = collider;
        }

        public void Show(TState from, TState to, float progress) //TODO: чужой, который сходит с едущей платформы или встаёт на неё, перескакивает на её путь за задержку (до скорость × ~0.1 с): правило одно с откатом на сервере, поэтому без сглаживания — сгладить только картинку, не трогая коллайдер
        {
            if (_controlledBy.Me.Value)
            {
                _view.Show(from, to, progress);
                return;
            }

            var duration = (double)_ticks.Duration;
            var shown = Vector2.Lerp(from.Body.Position, to.Body.Position, progress);
            var shift = _collider.RideShift(shown, _interpolation.InterpolationTick * duration, _ticks.PredictedViewTick(_interpolation) * duration);
            _view.Show(from.Shifted(shift), to.Shifted(shift), progress);
        }
    }
}
