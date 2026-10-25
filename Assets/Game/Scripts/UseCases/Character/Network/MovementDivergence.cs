using Bw.Entities.Network.Rollback;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Character.Network
{
    public sealed class MovementDivergence : IStateDivergence<MovementState>
    {
        private const float PositionTolerance = 0.01f;
        private const float VelocityTolerance = 0.1f;

        public bool Diverged(MovementState predicted, MovementState actual) =>
            (predicted.Body.Position - actual.Body.Position).sqrMagnitude > PositionTolerance * PositionTolerance
            || (predicted.Body.Velocity - actual.Body.Velocity).sqrMagnitude > VelocityTolerance * VelocityTolerance
            || !predicted.Body.HorizontalPush.Equals(actual.Body.HorizontalPush)
            || predicted.Body.Grounded != actual.Body.Grounded
            || !predicted.Jump.Equals(actual.Jump);
    }
}
