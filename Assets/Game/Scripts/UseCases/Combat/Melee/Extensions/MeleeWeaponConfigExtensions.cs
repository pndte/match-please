using Bw.Entities.Simulation;
using UnityEngine;

namespace Bw.UseCases.Combat.Melee.Extensions
{
    public static class MeleeWeaponConfigExtensions
    {
        public static int SwingTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Max(1, Mathf.CeilToInt(config.SwingTime / step.Duration));

        public static int StrikeTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Min(Mathf.RoundToInt(config.StrikeTime / step.Duration), config.SwingTicks(step) - 1);

        public static int StrongSwingTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Max(1, Mathf.CeilToInt(config.StrongSwingTime / step.Duration));

        public static int StrongStrikeTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Min(Mathf.RoundToInt(config.StrongStrikeTime / step.Duration), config.StrongSwingTicks(step) - 1);

        public static int MinChargeTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.CeilToInt(config.MinChargeTime / step.Duration);

        public static int FullChargeTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.Max(1, Mathf.CeilToInt(config.FullChargeTime / step.Duration));

        public static int FullChargeHoldTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.CeilToInt(config.FullChargeHoldTime / step.Duration);

        public static int RecoveryTicks(this MeleeWeaponConfig config, ISimulationStep step) =>
            Mathf.CeilToInt(config.RecoveryTime / step.Duration);

        public static float Charge(this MeleeWeaponConfig config, ISimulationStep step, int chargeTicks) =>
            Mathf.Min(1f, (float)chargeTicks / config.FullChargeTicks(step));
    }
}
