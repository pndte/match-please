using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class SpriteMovementAnimator : IMovementAnimator
    {
        private static readonly int SpeedId = Animator.StringToHash(MovementAnimatorParameters.Speed);
        private static readonly int VerticalSpeedId = Animator.StringToHash(MovementAnimatorParameters.VerticalSpeed);
        private static readonly int GroundedId = Animator.StringToHash(MovementAnimatorParameters.Grounded);

        private readonly Animator _animator;
        private readonly SpriteRenderer _sprite;

        public SpriteMovementAnimator(Animator animator, SpriteRenderer sprite)
        {
            _animator = animator;
            _sprite = sprite;
        }

        public void Animate(MovementAnimationPose pose)
        {
            _animator.SetFloat(SpeedId, pose.Speed);
            _animator.SetFloat(VerticalSpeedId, pose.VerticalSpeed);
            _animator.SetBool(GroundedId, pose.Grounded);
            _sprite.flipX = pose.Facing < 0f;
        }
    }
}
