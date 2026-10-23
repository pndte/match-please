using System;
using Bw.UseCases.Combat.Weapon.Abstractions;

namespace Bw.UseCases.Combat.Melee
{
    public readonly struct MeleeState : IWeaponState, IEquatable<MeleeState>
    {
        public readonly float Aim;
        public readonly MeleePhase Phase;
        public readonly int PhaseTicks;
        public readonly bool Held;
        public readonly int ChargeTicks;
        public readonly int Shots;

        float IWeaponState.Aim => Aim;
        int IWeaponState.Shots => Shots;
        bool IWeaponState.Stowable => Phase == MeleePhase.Idle || (Phase == MeleePhase.Jab && !Held);

        public MeleeState(
            float aim,
            MeleePhase phase,
            int phaseTicks,
            bool held,
            int chargeTicks,
            int shots)
        {
            Aim = aim;
            Phase = phase;
            PhaseTicks = phaseTicks;
            Held = held;
            ChargeTicks = chargeTicks;
            Shots = shots;
        }

        public bool Equals(MeleeState other) =>
            Aim.Equals(other.Aim)
            && Phase == other.Phase
            && PhaseTicks == other.PhaseTicks
            && Held == other.Held
            && ChargeTicks == other.ChargeTicks
            && Shots == other.Shots;
    }
}
