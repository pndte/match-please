using System;

namespace Bw.UseCases.Mortality.Guarding
{
    public readonly struct GuardState : IEquatable<GuardState>
    {
        public readonly float Health;
        public readonly bool Raised;
        public readonly bool Broken;
        public readonly float Aim;
        public readonly int TicksSinceBlow;
        public readonly int TicksSinceRegen;

        public bool Up => Raised && !Broken;

        public GuardState(
            float health,
            bool raised,
            bool broken,
            float aim,
            int ticksSinceBlow,
            int ticksSinceRegen)
        {
            Health = health;
            Raised = raised;
            Broken = broken;
            Aim = aim;
            TicksSinceBlow = ticksSinceBlow;
            TicksSinceRegen = ticksSinceRegen;
        }

        public bool Equals(GuardState other) =>
            Health == other.Health
            && Raised == other.Raised
            && Broken == other.Broken
            && Aim == other.Aim
            && TicksSinceBlow == other.TicksSinceBlow
            && TicksSinceRegen == other.TicksSinceRegen;

        public override bool Equals(object obj) =>
            obj is GuardState other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Health, Raised, Broken, Aim, TicksSinceBlow, TicksSinceRegen);
    }
}
