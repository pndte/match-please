using Bw.Entities.Simulation;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Ticks
{
    public interface INetworkTicks : ISimulationStep
    {
        public int Current { get; }
        public ISource<int> Ticked { get; }
    }
}
