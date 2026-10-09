using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Prediction.Events
{
    public interface IOutcomes<TInitiator, TTarget, TEffect> where TInitiator : class where TTarget : class where TEffect : struct
    {
        public ISource<ActionOutcome<TInitiator, TTarget, TEffect>> Reported { get; }
    }
}
