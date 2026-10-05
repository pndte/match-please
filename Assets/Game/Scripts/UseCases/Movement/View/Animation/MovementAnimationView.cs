using Bw.Entities.Simulation;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.View.Animation
{
    public sealed class MovementAnimationView : IStateView<MovementState>
    {
        private const float TurnSpeed = 0.05f;

        private readonly IMovementAnimator _animator;
        private readonly MovementConfig _config;
        private float _facing = 1f;

        public MovementAnimationView(IMovementAnimator animator, MovementConfig config)
        {
            _animator = animator;
            _config = config;
        }

        public void Show(MovementState from, MovementState to, float progress)
        {
            var velocity = Vector2.Lerp(from.Velocity, to.Velocity, progress);
            var speed = Mathf.Abs(velocity.x) / _config.Speed;
            if (speed > TurnSpeed)
                _facing = Mathf.Sign(velocity.x);

            var grounded = progress < 0.5f ? from.Grounded : to.Grounded;
            _animator.Animate(new MovementAnimationPose(speed, velocity.y / _config.JumpForce, grounded, _facing));
        }
    }
}
