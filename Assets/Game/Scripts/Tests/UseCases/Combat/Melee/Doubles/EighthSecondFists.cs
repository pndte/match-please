using Bw.UseCases.Combat.Melee;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Melee.Doubles
{
    public static class EighthSecondFists
    {
        public const float Step = 0.125f;
        public const int SwingTicks = 4;
        public const int StrikeTicks = 2;
        public const int StrongSwingTicks = 6;
        public const int StrongStrikeTicks = 2;
        public const int MinChargeTicks = 4;
        public const int FullChargeTicks = 8;
        public const int FullChargeHoldTicks = 4;
        public const int RecoveryTicks = 2;
        public const float TurnPerTick = 10f;

        public static MeleeWeaponConfig Create()
        {
            var config = ScriptableObject.CreateInstance<MeleeWeaponConfig>();
            config.SwingTime = SwingTicks * Step;
            config.StrikeTime = StrikeTicks * Step;
            config.StrongSwingTime = StrongSwingTicks * Step;
            config.StrongStrikeTime = StrongStrikeTicks * Step;
            config.MinChargeTime = MinChargeTicks * Step;
            config.FullChargeTime = FullChargeTicks * Step;
            config.FullChargeHoldTime = FullChargeHoldTicks * Step;
            config.RecoveryTime = RecoveryTicks * Step;
            config.TurnSpeed = TurnPerTick / Step;
            return config;
        }
    }
}
