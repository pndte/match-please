using Bw.Entities.Extensions;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class BodyStateExtensions
    {
        public static BodyState Pushed(this BodyState state, Vector2 impulse) =>
            new(state.Position,
                state.Velocity.WithY(state.Velocity.y + impulse.y),
                state.HorizontalPush + impulse.x,
                state.Grounded);

        public static BodyState Shifted(this BodyState state, Vector2 offset) =>
            new(state.Position + offset, state.Velocity, state.HorizontalPush, state.Grounded);

        public static TState Pushed<TState>(this TState state, Vector2 impulse) where TState : struct, IBodyState<TState> =>
            state.WithBody(state.Body.Pushed(impulse));

        public static TState Shifted<TState>(this TState state, Vector2 offset) where TState : struct, IBodyState<TState> =>
            state.WithBody(state.Body.Shifted(offset));
    }
}
