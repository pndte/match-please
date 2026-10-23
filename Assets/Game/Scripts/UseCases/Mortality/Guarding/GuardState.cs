using System;

namespace Bw.UseCases.Mortality.Guarding
{
    public readonly struct GuardState : IEquatable<GuardState>
    {
        public readonly float Health;
        public readonly bool Raised;
        public readonly bool Broken;

        public bool Up => Raised && !Broken;

        public GuardState(float health, bool raised, bool broken)
        {
            Health = health;
            Raised = raised;
            Broken = broken;
        }

        public bool Equals(GuardState other) =>
            Health == other.Health && Raised == other.Raised && Broken == other.Broken;

        public override bool Equals(object obj) =>
            obj is GuardState other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Health, Raised, Broken);
    }
}
