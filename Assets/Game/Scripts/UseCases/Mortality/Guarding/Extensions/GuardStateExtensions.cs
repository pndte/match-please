using UnityEngine;

namespace Bw.UseCases.Mortality.Guarding.Extensions
{
    public static class GuardStateExtensions
    {
        public static GuardState Faced(this GuardState state, GuardStance stance) =>
            new(state.Health, stance.Raised, state.Broken, stance.Aim, state.TicksSinceBlow, state.TicksSinceRegen);

        public static GuardState Counted(this GuardState state, int restTicks) =>
            new(state.Health, state.Raised, state.Broken, state.Aim, Mathf.Min(state.TicksSinceBlow + 1, restTicks), Mathf.Min(state.TicksSinceRegen + 1, restTicks));

        public static GuardState AfterBlow(this GuardState state, float damage) =>
            new(Mathf.Max(0f, state.Health - damage), state.Raised, state.Broken, state.Aim, 0, state.TicksSinceRegen);

        public static GuardState AsBroken(this GuardState state) =>
            new(0f, state.Raised, true, state.Aim, state.TicksSinceBlow, state.TicksSinceRegen);

        public static GuardState Mended(this GuardState state, float health) =>
            new(health, state.Raised, false, state.Aim, state.TicksSinceBlow, 0);

        public static GuardState AfterRegen(this GuardState state, float amount, float max) =>
            new(Mathf.Min(max, state.Health + amount), state.Raised, state.Broken, state.Aim, state.TicksSinceBlow, 0);
    }
}
