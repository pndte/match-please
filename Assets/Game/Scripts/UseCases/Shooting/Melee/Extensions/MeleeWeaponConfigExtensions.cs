using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Shooting.Melee.Extensions
{
    public static class MeleeWeaponConfigExtensions
    {
        public static int SwingTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Max(1, Mathf.CeilToInt(config.SwingTime / step.Duration));

        public static int StrikeTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Min(Mathf.RoundToInt(config.StrikeTime / step.Duration), config.SwingTicks(step) - 1);
    }
}
