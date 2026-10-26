using Bw.Entities.Simulation;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class MovementAnimationView : IStateView<MovementMotion>
    {
        private const float MovingSpeed = 0.05f;

        private readonly IMovementAnimator _animator;
        private readonly IFacingViewModel _facing;

        public MovementAnimationView(IMovementAnimator animator, IFacingViewModel facing)
        {
            _animator = animator;
            _facing = facing;
        }

        public void Show(MovementMotion from, MovementMotion to, float progress)
        {
            var run = Mathf.Lerp(from.Run, to.Run, progress);
            var speed = Mathf.Abs(run);
            var facing = _facing.Facing;
            var backward = speed > MovingSpeed && Mathf.Sign(run) != facing;
            var grounded = progress < 0.5f ? from.Grounded : to.Grounded;
            _animator.Animate(new MovementAnimationPose(speed, Mathf.Lerp(from.Rise, to.Rise, progress), grounded, facing, backward));
        }
    }
}
