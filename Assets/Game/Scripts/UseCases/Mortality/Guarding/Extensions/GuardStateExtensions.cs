using UnityEngine;

namespace Bw.UseCases.Mortality.Guarding.Extensions
{
    public static class GuardStateExtensions
    {
        public static GuardState WithRaised(this GuardState state, bool raised) =>
            new(state.Health, raised, state.Broken);

        public static GuardState AfterBlow(this GuardState state, float damage) =>
            new(Mathf.Max(0f, state.Health - damage), state.Raised, state.Broken);

        public static GuardState AsBroken(this GuardState state) =>
            new(0f, state.Raised, true);

        public static GuardState Mended(this GuardState state, float health) =>
            new(health, state.Raised, false);

        public static GuardState AfterRegen(this GuardState state, float amount, float max) =>
            new(Mathf.Min(max, state.Health + amount), state.Raised, state.Broken);
    }
}
