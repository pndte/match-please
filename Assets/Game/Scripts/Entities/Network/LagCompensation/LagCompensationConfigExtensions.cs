using System;
using Bw.Entities.Simulation;

namespace Bw.Entities.Network.LagCompensation
{
    public static class LagCompensationConfigExtensions
    {
        public static int RewindTicks(this LagCompensationConfig config, ISimulationStep step) =>
            (int)Math.Ceiling(config.MaxRewindSeconds / step.Duration);
    }
}
