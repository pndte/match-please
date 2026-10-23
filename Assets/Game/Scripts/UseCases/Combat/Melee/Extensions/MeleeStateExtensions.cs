using Bw.Entities.Simulation;

namespace Bw.UseCases.Combat.Melee.Extensions
{
    public static class MeleeStateExtensions
    {
        public static bool FullyCharged(this MeleeState state, MeleeWeaponConfig config, ISimulationStep step) =>
            state.Phase == MeleePhase.Charging && state.ChargeTicks >= config.FullChargeTicks(step);

        public static bool StrongSwingStarted(this MeleeState state) =>
            state.Phase == MeleePhase.StrongSwing && state.PhaseTicks == 0;

        public static bool StrikePending(this MeleeState state, MeleeWeaponConfig config, ISimulationStep step) =>
            state.Phase switch
            {
                MeleePhase.Jab => state.PhaseTicks < config.StrikeTicks(step),
                MeleePhase.StrongSwing => state.PhaseTicks < config.StrongStrikeTicks(step),
                _ => false
            };
    }
}
