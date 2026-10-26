using Bw.Entities.Network.Rollback;
using Bw.UseCases.Mortality.Guarding;
using UnityEngine;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardDivergence : IStateDivergence<GuardState>
    {
        private const float AimTolerance = 0.01f;

        public bool Diverged(GuardState predicted, GuardState actual) =>
            predicted.Health != actual.Health
            || predicted.Raised != actual.Raised
            || predicted.Broken != actual.Broken
            || predicted.TicksSinceBlow != actual.TicksSinceBlow
            || predicted.TicksSinceRegen != actual.TicksSinceRegen
            || Mathf.Abs(Mathf.DeltaAngle(predicted.Aim, actual.Aim)) > AimTolerance;
    }
}
