using System;

namespace Bw.UseCases.Mortality.View.Health
{
    public readonly struct HealthLevel : IEquatable<HealthLevel>
    {
        public static readonly HealthLevel Full = new(1f, 0f);

        public readonly float Share;
        public readonly float Change;

        public HealthLevel(float share, float change)
        {
            Share = share;
            Change = change;
        }

        public bool Equals(HealthLevel other) =>
            Share == other.Share && Change == other.Change;
    }
}
