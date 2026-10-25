using Bw.Entities.Network.Prediction.Events;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class ManualOutcomes<TInitiator, TTarget, TEffect> : IOutcomes<TInitiator, TTarget, TEffect>
        where TInitiator : class
        where TTarget : class
        where TEffect : struct
    {
        public ISource<ActionOutcome<TInitiator, TTarget, TEffect>> Reported => _reported;

        private readonly Signal<ActionOutcome<TInitiator, TTarget, TEffect>> _reported = new();

        public void Report(ActionOutcome<TInitiator, TTarget, TEffect> outcome) =>
            _reported.Fire(outcome);
    }
}
