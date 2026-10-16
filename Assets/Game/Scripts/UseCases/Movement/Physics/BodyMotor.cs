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
        private const float Touching = 0.001f;

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
            var launchHeight = 0f;
            launched = state.Grounded && _collider.TryGetLaunch(position, out launchHeight);

            var velocity = launched ? drive.Velocity.WithY(LaunchSpeed(launchHeight)) : drive.Velocity;
            var gravityScale = launched ? 1f : drive.GravityScale;
            velocity = velocity.WithY(velocity.y + _gravity * gravityScale * deltaTime);

            var push = state.HorizontalPush;
            var horizontal = velocity.x + push;
            if (horizontal != 0f)
            {
                var direction = Mathf.Sign(horizontal);
                var distance = Mathf.Abs(horizontal) * deltaTime;
                var way = new Vector2(direction, 0f);
                var travel = _collider.Cast(position, way, distance);
                if (Walking(way, drive))
                    Lean(position, way, travel, Mathf.Abs(drive.Velocity.x));

                position += way * travel;
                if (travel < distance && Mathf.Sign(push) == direction)
                    push = 0f;
            }

            push = _config.DeceleratedPush(push, state.Grounded, _step);

            var verticalDirection = velocity.y > 0f ? Vector2.up : Vector2.down;
            var verticalDistance = Mathf.Abs(velocity.y) * deltaTime;
            var travelled = verticalDirection == Vector2.down && !drive.Dropping
                ? Fall(ref position, verticalDistance)
                : Sweep(ref position, verticalDirection, verticalDistance);

            if (travelled >= verticalDistance)
                return new BodyState(position, velocity, push, false);

            return new BodyState(position, velocity.WithY(0f), push, verticalDirection == Vector2.down);
        }

        private float LaunchSpeed(float height) =>
            -_gravity * _step.Duration / 2f + Mathf.Sqrt(-2f * _gravity * height);

        private void Lean(
            Vector2 position,
            Vector2 direction,
            float travel,
            float speed)
        {
            var reach = travel + Touching;
            var contacts = _collider.ShovablesAhead(position, direction, reach);
            var reached = 0;
            for (var index = 0; index < contacts.Count; index++)
                if (contacts[index].Gap <= reach)
                    reached++;

            for (var index = 0; index < contacts.Count; index++)
            {
                var contact = contacts[index];
                var effort = 1f - contact.Shovable.RetreatSpeed(direction) / speed;
                if (contact.Gap <= reach && effort > 0f)
                    contact.Shovable.Shove(direction * (_config.ShoveForce * effort / reached));
            }
        }

        private static bool Walking(Vector2 direction, BodyDrive drive) =>
            drive.Velocity.x * direction.x > 0f;

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
