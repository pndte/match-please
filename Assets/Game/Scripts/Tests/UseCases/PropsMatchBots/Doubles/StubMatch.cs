using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class StubMatch : IReadonlyMatch
    {
        public IReadonlyProperty<MatchPeriod> Period => _period;

        private readonly ViewableProperty<MatchPeriod> _period = new(new MatchPeriod(MatchPhase.Running, 1000));

        public void Enter(MatchPeriod period) =>
            _period.Value = period;
    }
}
