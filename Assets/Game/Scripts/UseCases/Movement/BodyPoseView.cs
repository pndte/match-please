using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public sealed class BodyPoseView<TState> : IStateView<TState> where TState : struct, IPlacedState<TState>
    {
        private readonly IBodyPose _body;
        private readonly IBodyPicture _picture;

        public BodyPoseView(IBodyPose body, IBodyPicture picture)
        {
            _body = body;
            _picture = picture;
        }

        public void Show(TState from, TState to, float progress)
        {
            var position = Vector2.Lerp(from.Position, to.Position, progress);
            _body.Place(position, Mathf.LerpAngle(from.Angle, to.Angle, progress));
            _picture.Place(position);
        }
    }
}
