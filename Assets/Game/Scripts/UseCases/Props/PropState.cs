using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Props
{
    public readonly struct PropState : IWeaponState, IEquatable<PropState>
    {
        public readonly float Aim;
        public readonly PropPhase Phase;
        public readonly int PhaseTicks;
        public readonly int Throws;
        public readonly bool TriggerHeld;

        float IWeaponState.Aim => Aim;
        int IWeaponState.Shots => Throws;
        bool IWeaponState.Stowable => Phase == PropPhase.Holding;

        public PropState(
            float aim,
            PropPhase phase,
            int phaseTicks,
            int throws,
            bool triggerHeld)
        {
            Aim = aim;
            Phase = phase;
            PhaseTicks = phaseTicks;
            Throws = throws;
            TriggerHeld = triggerHeld;
        }

        public bool Equals(PropState other) =>
            Aim.Equals(other.Aim)
            && Phase == other.Phase
            && PhaseTicks == other.PhaseTicks
            && Throws == other.Throws
            && TriggerHeld == other.TriggerHeld;
    }
}
