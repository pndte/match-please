using System.Collections.Generic;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class ActionOutcome<TInitiator, TTarget, TEffect> where TInitiator : class where TTarget : class where TEffect : struct
    {
        public TInitiator Initiator { get; }
        public int Tick { get; }
        public IReadOnlyList<AffectedTarget<TTarget, TEffect>> Affected { get; }

        public ActionOutcome(TInitiator initiator, int tick, IReadOnlyList<AffectedTarget<TTarget, TEffect>> affected)
        {
            Initiator = initiator;
            Tick = tick;
            Affected = affected;
        }
    }

    public readonly struct AffectedTarget<TTarget, TEffect> where TEffect : struct
    {
        public readonly TTarget Target;
        public readonly TEffect Effect;

        public AffectedTarget(TTarget target, TEffect effect)
        {
            Target = target;
            Effect = effect;
        }
    }
}
