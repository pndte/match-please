using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Movement.Extensions
{
    public static class MovementConfigExtensions
    {
        public static int CoyoteTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.CoyoteTime, step);

        public static int JumpBufferTicks(this MovementConfig config, ISimulationStep step) =>
            Ticks(config.JumpBufferTime, step);

        private static int Ticks(float seconds, ISimulationStep step) =>
            Mathf.RoundToInt(seconds / step.Duration);
    }
}
