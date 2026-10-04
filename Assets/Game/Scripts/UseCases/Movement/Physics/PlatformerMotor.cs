using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class PlatformerMotor : IMovementMotor
    {
        private readonly IMovementCollider _collider;
        private readonly ISimulationStep _step;
        private readonly MovementConfig _config;
        private readonly float _gravity;

        public PlatformerMotor(IMovementCollider collider, ISimulationStep step, MovementConfig config)
        {
            _collider = collider;
            _step = step;
            _config = config;
            _gravity = Physics2D.gravity.y * config.GravityScale;
        }

        public MovementState Step(MovementState state, MovementInput input)
        {
            var deltaTime = _step.Duration;

            var velocity = state.Velocity.WithX(Mathf.Clamp(input.Horizontal, -1f, 1f) * _config.Speed);
            if (input.Jump && state.Grounded)
                velocity = velocity.WithY(_config.JumpForce);
            velocity = velocity.WithY(velocity.y + _gravity * deltaTime);

            var position = state.Position;
            if (velocity.x != 0f)
                Sweep(ref position, new Vector2(Mathf.Sign(velocity.x), 0f), Mathf.Abs(velocity.x) * deltaTime);

            var verticalDirection = velocity.y > 0f ? Vector2.up : Vector2.down;
            var verticalDistance = Mathf.Abs(velocity.y) * deltaTime;
            if (Sweep(ref position, verticalDirection, verticalDistance) >= verticalDistance)
                return new MovementState(position, velocity, false);

            return new MovementState(position, velocity.WithY(0f), verticalDirection == Vector2.down);
        }

        private float Sweep(ref Vector2 position, Vector2 direction, float distance)
        {
            var allowed = _collider.Cast(position, direction, distance);
            position += direction * allowed;
            return allowed;
        }
    }
}
