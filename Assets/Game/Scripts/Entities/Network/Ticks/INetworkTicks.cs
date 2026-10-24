using Bw.Entities.Simulation;

namespace Bw.Entities.Network.Ticks
{
    public interface INetworkTicks : ISimulationStep
    {
        public int Current { get; }
    }
}
