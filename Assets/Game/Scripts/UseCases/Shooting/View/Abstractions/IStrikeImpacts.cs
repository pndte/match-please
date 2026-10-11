using Bw.UseCases.Shooting.View.Impact;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IStrikeImpacts
    {
        public ISource<StrikeImpact> Bodies { get; }
        public ISource<StrikeImpact> Surfaces { get; }
    }
}
