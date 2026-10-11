using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public interface IMeleeSwings
    {
        public ISource<int> Started { get; }
    }
}
