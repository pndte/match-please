using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class ShovedStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IStateView<TState> _view;
        private readonly IPredictedShove _shove;

        public ShovedStateView(IStateView<TState> view, IPredictedShove shove)
        {
            _view = view;
            _shove = shove;
        }

        public void Show(TState from, TState to, float progress)
        {
            if (!_shove.TryGetShown(out var position))
            {
                _view.Show(from, to, progress);
                return;
            }

            var offset = position - Vector2.Lerp(from.Body.Position, to.Body.Position, progress);
            _view.Show(from.Shifted(offset), to.Shifted(offset), progress);
        }
    }
}
