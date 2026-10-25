using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class BodyStateView<TState> : IStateView<TState> where TState : struct, IBodyState<TState>
    {
        private readonly IBodyPicture _picture;

        public BodyStateView(IBodyPicture picture)
        {
            _picture = picture;
        }

        public void Show(TState from, TState to, float progress) =>
            _picture.Place(Vector2.Lerp(from.Body.Position, to.Body.Position, progress));
    }
}
