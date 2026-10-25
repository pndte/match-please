using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class RigMovementAnimator : IMovementAnimator
    {
        private static readonly int SpeedId = Animator.StringToHash(MovementAnimatorParameters.Speed);
        private static readonly int VerticalSpeedId = Animator.StringToHash(MovementAnimatorParameters.VerticalSpeed);
        private static readonly int GroundedId = Animator.StringToHash(MovementAnimatorParameters.Grounded);
        private static readonly int BackwardId = Animator.StringToHash(MovementAnimatorParameters.Backward);

        private readonly Animator _animator;
        private readonly Transform _rig;

        public RigMovementAnimator(Animator animator, Transform rig)
        {
            _animator = animator;
            _rig = rig;
        }

        public void Animate(MovementAnimationPose pose)
        {
            _animator.SetFloat(SpeedId, pose.Speed);
            _animator.SetFloat(VerticalSpeedId, pose.VerticalSpeed);
            _animator.SetBool(GroundedId, pose.Grounded);
            _animator.SetBool(BackwardId, pose.Backward);
            var scale = _rig.localScale;
            _rig.localScale = new Vector3(Mathf.Abs(scale.x) * (pose.Facing < 0f ? -1f : 1f), scale.y, scale.z);
        }
    }
}
