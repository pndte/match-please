using Bw.Entities.Simulation;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class MovementAnimationView : IStateView<MovementMotion>
    {
        private const float TurnSpeed = 0.05f;

        private readonly IMovementAnimator _animator;
        private float _facing = 1f;

        public MovementAnimationView(IMovementAnimator animator)
        {
            _animator = animator;
        }

        public void Show(MovementMotion from, MovementMotion to, float progress)
        {
            var run = Mathf.Lerp(from.Run, to.Run, progress);
            var speed = Mathf.Abs(run); //TODO: скорость бега берётся из управления, а не из пройденного: персонаж, который толкает ящик или упёрся в стену, бежит на месте полным шагом
            if (speed > TurnSpeed)
                _facing = Mathf.Sign(run);

            var grounded = progress < 0.5f ? from.Grounded : to.Grounded;
            _animator.Animate(new MovementAnimationPose(speed, Mathf.Lerp(from.Rise, to.Rise, progress), grounded, _facing));
        }
    }
}
