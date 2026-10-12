using System;
using Bw.UseCases.Movement.Abstractions;

namespace Bw.UseCases.Movement
{
    public readonly struct MovementState : IBodyState<MovementState>, IEquatable<MovementState>
    {
        public readonly BodyState Body;
        public readonly JumpState Jump;

        BodyState IBodyState<MovementState>.Body => Body;
        MovementState IBodyState<MovementState>.WithBody(BodyState body) => new(body, Jump);

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
