using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Ticks
{
    public interface IWorldClock
    {
        public ISource<int> Ticked { get; }
    }
}
