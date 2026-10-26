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
            var jumps = JumpWanted(state, input) && JumpAllowed(state);
            var rising = jumps || state.Jump.Rising;
            var velocity = new Vector2(
                RunSpeed(state.Body.Velocity.x, Mathf.Clamp(input.Horizontal, -1f, 1f) * Speed(input)),
                jumps ? JumpForce(input) : state.Body.Velocity.y);

            var body = _body.Step(state.Body, new BodyDrive(velocity, GravityScale(velocity, rising, input), input.Down), out var launched);
            var coyoteTicks = jumps || launched ? 0 : NextCoyoteTicks(state);
            var bufferedTicks = jumps ? 0 : NextBufferedTicks(state, input);
            return new MovementState(body, new JumpState(!launched && rising && body.Velocity.y > 0f, coyoteTicks, bufferedTicks));
        }

        private float Speed(MovementInput input) =>
            _config.Speed * (1f - input.Slowdown.Speed);

        private float RunSpeed(float current, float wanted)
        {
            var time = Braking(current, wanted) ? _config.BrakingTime : _config.AccelerationTime;
            return time > 0f ? Mathf.MoveTowards(current, wanted, _config.Speed / time * _step.Duration) : wanted;
        }

        private static bool Braking(float current, float wanted) =>
            Mathf.Abs(wanted) < Mathf.Abs(current) || wanted * current < 0f;

        private float JumpForce(MovementInput input) =>
            _config.JumpForce * Mathf.Sqrt(1f - input.Slowdown.JumpHeight);

        private static bool JumpWanted(MovementState state, MovementInput input) =>
            input.JumpPressed || state.Jump.BufferedTicks > 0;

        private static bool JumpAllowed(MovementState state) =>
            state.Body.Grounded || state.Jump.CoyoteTicks > 0;

        private int NextCoyoteTicks(MovementState state) =>
            state.Body.Grounded ? _config.CoyoteTicks(_step) : Countdown(state.Jump.CoyoteTicks);

        private int NextBufferedTicks(MovementState state, MovementInput input) =>
            input.JumpPressed ? _config.JumpBufferTicks(_step) : Countdown(state.Jump.BufferedTicks);

        private static int Countdown(int ticks) =>
            Mathf.Max(0, ticks - 1);

        private float GravityScale(Vector2 velocity, bool rising, MovementInput input)
        {
            if (velocity.y < 0f)
                return _config.FallGravity;

            return rising && velocity.y > 0f && !input.JumpHeld ? _config.ReleasedJumpGravity : 1f;
        }
    }
}
