using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;
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
            var jumps = WantsToJump(state, input) && CanJump(state);
            var coyoteTicks = jumps ? 0 : NextCoyoteTicks(state);
            var bufferedTicks = jumps ? 0 : NextBufferedTicks(state, input);
            var rising = jumps || state.Jump.Rising;

            var velocity = state.Velocity.WithX(Mathf.Clamp(input.Horizontal, -1f, 1f) * _config.Speed);
            if (jumps)
                velocity = velocity.WithY(_config.JumpForce);

            velocity = velocity.WithY(velocity.y + Gravity(velocity, rising, input) * deltaTime);

            var position = state.Position;
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
            if (Sweep(ref position, verticalDirection, verticalDistance) >= verticalDistance)
                return new MovementState(position, velocity, push, false, new JumpState(rising && velocity.y > 0f, coyoteTicks, bufferedTicks));

            return new MovementState(position, velocity.WithY(0f), push, verticalDirection == Vector2.down, new JumpState(false, coyoteTicks, bufferedTicks));
        }

        private static bool WantsToJump(MovementState state, MovementInput input) =>
            input.Jump || state.Jump.BufferedTicks > 0;

        private static bool CanJump(MovementState state) =>
            state.Grounded || state.Jump.CoyoteTicks > 0;

        private int NextCoyoteTicks(MovementState state) =>
            state.Grounded ? _config.CoyoteTicks(_step) : Countdown(state.Jump.CoyoteTicks);

        private int NextBufferedTicks(MovementState state, MovementInput input) =>
            input.Jump ? _config.JumpBufferTicks(_step) : Countdown(state.Jump.BufferedTicks);

        private static int Countdown(int ticks) =>
            Mathf.Max(0, ticks - 1);

        private float Gravity(Vector2 velocity, bool rising, MovementInput input) => //TODO: толчок вверх посреди собственного прыжка срезается вместе с прыжком, если отпустить кнопку: фаза прыжка не знает, какая часть скорости от толчка
            rising && velocity.y > 0f && !input.JumpHeld ? _gravity * _config.ReleasedJumpGravity : _gravity;

        private float Sweep(ref Vector2 position, Vector2 direction, float distance)
        {
            var allowed = _collider.Cast(position, direction, distance);
            position += direction * allowed;
            return allowed;
        }
    }
}
