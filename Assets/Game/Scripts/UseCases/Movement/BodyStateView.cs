using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class BodyStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IMovementBody _body;

        public BodyStateView(IMovementBody body)
        {
            _body = body;
        }

        public void Show(TState from, TState to, float progress) =>
            _body.Place(Vector2.Lerp(from.Body.Position, to.Body.Position, progress));
    }
}
