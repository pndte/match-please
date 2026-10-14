using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Match.Network
{
    public interface IReadonlyMatch
    {
        public IReadonlyProperty<MatchPeriod> Period { get; }
    }
}
