using System;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using UnityEngine;

namespace Bw.UseCases.Movement
{
    public readonly struct MovementState : IBodyState<MovementState>, IEquatable<MovementState>
    {
        public readonly BodyState Body;
        public readonly JumpState Jump;

        BodyState IBodyState<MovementState>.Body => Body;
        MovementState IBodyState<MovementState>.WithBody(BodyState body) => new(body, Jump);
        Vector2 IPlacedState<MovementState>.Position => Body.Position;
        float IPlacedState<MovementState>.Angle => 0f;
        MovementState IPlacedState<MovementState>.Shifted(Vector2 offset) => new(Body.Shifted(offset), Jump);

        public MovementState(BodyState body, JumpState jump)
        {
            Body = body;
            Jump = jump;
        }

        public bool Equals(MovementState other) =>
            Body.Equals(other.Body)
            && Jump.Equals(other.Jump);
    }
}
