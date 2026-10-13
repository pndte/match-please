using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class PlatformerMotor : IMovementMotor
    {
        private readonly IBodyMotor _body;
        private readonly ISimulationStep _step;
        private readonly MovementConfig _config;

        public PlatformerMotor(IBodyMotor body, ISimulationStep step, MovementConfig config)
        {
            _body = body;
            _step = step;
            _config = config;
        }

        public MovementState Step(MovementState state, MovementInput input)
        {
            var jumps = WantsToJump(state, input) && CanJump(state);
            var rising = jumps || state.Jump.Rising;
            var velocity = new Vector2(
                Mathf.Clamp(input.Horizontal, -1f, 1f) * _config.Speed,
                jumps ? _config.JumpForce : state.Body.Velocity.y);

            var body = _body.Step(state.Body, new BodyDrive(velocity, GravityScale(velocity, rising, input), input.Down, _config.ShoveSpeed), out var launched);
            var coyoteTicks = jumps || launched ? 0 : NextCoyoteTicks(state);
            var bufferedTicks = jumps ? 0 : NextBufferedTicks(state, input);
            return new MovementState(body, new JumpState(!launched && rising && body.Velocity.y > 0f, coyoteTicks, bufferedTicks));
        }

        private static bool WantsToJump(MovementState state, MovementInput input) =>
            input.Jump || state.Jump.BufferedTicks > 0;

        private static bool CanJump(MovementState state) =>
            state.Body.Grounded || state.Jump.CoyoteTicks > 0;

        private int NextCoyoteTicks(MovementState state) =>
            state.Body.Grounded ? _config.CoyoteTicks(_step) : Countdown(state.Jump.CoyoteTicks);

        private int NextBufferedTicks(MovementState state, MovementInput input) =>
            input.Jump ? _config.JumpBufferTicks(_step) : Countdown(state.Jump.BufferedTicks);

        private static int Countdown(int ticks) =>
            Mathf.Max(0, ticks - 1);

        private float GravityScale(Vector2 velocity, bool rising, MovementInput input) => //TODO: толчок вверх посреди собственного прыжка срезается вместе с прыжком, если отпустить кнопку: фаза прыжка не знает, какая часть скорости от толчка
            rising && velocity.y > 0f && !input.JumpHeld ? _config.ReleasedJumpGravity : 1f;
    }
}
