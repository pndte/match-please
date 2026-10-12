using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class BodyMotor : IBodyMotor
    {
        private readonly IMovementCollider _collider;
        private readonly ISimulationStep _step;
        private readonly IBodyConfig _config;
        private readonly float _gravity;

        public BodyMotor(IMovementCollider collider, ISimulationStep step, IBodyConfig config)
        {
            _collider = collider;
            _step = step;
            _config = config;
            _gravity = Physics2D.gravity.y * config.GravityScale;
        }

        public BodyState Step(BodyState state, BodyDrive drive, out bool launched)
        {
            var deltaTime = _step.Duration;
            var position = _collider.Carry(state.Position, state.Grounded);
            var launchSpeed = 0f;
            launched = state.Grounded && _collider.TryGetLaunch(position, out launchSpeed);

            var velocity = launched ? drive.Velocity.WithY(launchSpeed) : drive.Velocity;
            var gravityScale = launched ? 1f : drive.GravityScale;
            velocity = velocity.WithY(velocity.y + _gravity * gravityScale * deltaTime);

            var push = state.HorizontalPush;
            var horizontal = velocity.x + push;
            if (horizontal != 0f)
            {
                var direction = Mathf.Sign(horizontal);
                var distance = Mathf.Abs(horizontal) * deltaTime;
                if (Sweep(ref position, new Vector2(direction, 0f), distance) < distance && Mathf.Sign(push) == direction)
                    push = 0f;
            }

            push = _config.DeceleratedPush(push, _step);

            var verticalDirection = velocity.y > 0f ? Vector2.up : Vector2.down;
            var verticalDistance = Mathf.Abs(velocity.y) * deltaTime;
            var travelled = verticalDirection == Vector2.down && !drive.Dropping
                ? Fall(ref position, verticalDistance)
                : Sweep(ref position, verticalDirection, verticalDistance);

            if (travelled >= verticalDistance)
                return new BodyState(position, velocity, push, false);

            return new BodyState(position, velocity.WithY(0f), push, verticalDirection == Vector2.down);
        }

        private float Sweep(ref Vector2 position, Vector2 direction, float distance)
        {
            var allowed = _collider.Cast(position, direction, distance);
            position += direction * allowed;
            return allowed;
        }

        private float Fall(ref Vector2 position, float distance)
        {
            var allowed = _collider.Fall(position, distance);
            position += Vector2.down * allowed;
            return allowed;
        }
    }
}
