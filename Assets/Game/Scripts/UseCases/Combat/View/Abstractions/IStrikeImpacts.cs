using Bw.UseCases.Combat.View.Impact;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IStrikeImpacts
    {
        public ISource<StrikeImpact> Bodies { get; }
        public ISource<StrikeImpact> Surfaces { get; }
    }
}
