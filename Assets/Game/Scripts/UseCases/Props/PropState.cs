using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Props
{
    public readonly struct PropState : IWeaponState, IEquatable<PropState>
    {
        public readonly float Aim;
        public readonly int Throws;
        public readonly bool TriggerHeld;

        float IWeaponState.Aim => Aim;
        int IWeaponState.Shots => Throws;
        bool IWeaponState.Stowable => true;

        public PropState(float aim, int throws, bool triggerHeld)
        {
            Aim = aim;
            Throws = throws;
            TriggerHeld = triggerHeld;
        }

        public bool Equals(PropState other) =>
            Aim.Equals(other.Aim)
            && Throws == other.Throws
            && TriggerHeld == other.TriggerHeld;
    }
}
