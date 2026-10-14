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

        public static HealthState AfterHeal(this HealthState state, float amount, float max)
        {
            if (amount <= 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "A heal must restore health.");

            var current = Math.Min(max, state.Current + amount);
            return new HealthState(current, current - state.Current);
        }

        public static HealthState WithoutChange(this HealthState state) =>
            new(state.Current, 0f);
    }
}
