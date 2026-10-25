using Bw.Entities.Network.Rollback;
using UnityEngine;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public sealed class RigidDivergence : IStateDivergence<RigidState>
    {
        private const float Distance = 0.01f;
        private const float Turn = 1f;
        private const float Speed = 0.1f;

        public bool Diverged(RigidState predicted, RigidState actual) =>
            Vector2.Distance(predicted.Position, actual.Position) > Distance
            || Mathf.Abs(Mathf.DeltaAngle(predicted.Angle, actual.Angle)) > Turn
            || Vector2.Distance(predicted.Velocity, actual.Velocity) > Speed;
    }
}
