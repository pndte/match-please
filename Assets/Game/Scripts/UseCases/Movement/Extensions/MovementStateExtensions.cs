using Bw.Entities.Extensions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementStateExtensions
    {
        public static MovementState Pushed(this MovementState state, Vector2 impulse) =>
            new(state.Position,
                state.Velocity.WithY(state.Velocity.y + impulse.y),
                state.HorizontalPush + impulse.x,
                state.Grounded,
                state.RisingFromJump && impulse.y <= 0f,
                state.CoyoteTicks,
                state.BufferedJumpTicks);

        public static MovementState Shifted(this MovementState state, Vector2 offset) =>
            new(state.Position + offset,
                state.Velocity,
                state.HorizontalPush,
                state.Grounded,
                state.RisingFromJump,
                state.CoyoteTicks,
                state.BufferedJumpTicks);
    }
}
