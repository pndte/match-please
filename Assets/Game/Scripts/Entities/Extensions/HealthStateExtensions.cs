using System;

namespace Bw.Entities.Extensions
{
    public static class HealthStateExtensions
    {
        public static HealthState AfterHit(this HealthState state, float damage)
        {
            if (damage <= 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "A hit must deal damage.");

            var current = Math.Max(0f, state.Current - damage);
            return new HealthState(current, current - state.Current);
        }

        public static HealthState WithoutChange(this HealthState state) =>
            new(state.Current, 0f);
    }
}
