using System;

namespace Bw.Entities
{
    public readonly struct HealthState : IEquatable<HealthState>
    {
        public readonly float Current;
        public readonly float Change;

        public HealthState(float current, float change)
        {
            Current = current;
            Change = change;
        }

        public bool Equals(HealthState other) =>
            Current.Equals(other.Current) && Change.Equals(other.Change);
    }
}
