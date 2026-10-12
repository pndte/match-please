using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class MortalExtensions
    {
        public static void WhenDead(this IReadonlyProperty<LifeState> lifeState, Lifetime lifetime, Action<Lifetime> handler)
        {
            lifeState.View(lifetime, (stateLifetime, state) =>
            {
                if (state == LifeState.Dead) handler(stateLifetime);
            });
        }

        public static void WhenAlive(this IReadonlyProperty<LifeState> lifeState, Lifetime lifetime, Action<Lifetime> handler)
        {
            lifeState.View(lifetime, (stateLifetime, state) =>
            {
                if (state == LifeState.Alive) handler(stateLifetime);
            });
        }

        public static void AdviseKilled(this IReadonlyMortal mortal, Lifetime lifetime, Action handler)
        {
            var previous = mortal.Current;
            mortal.Applied.Advise(lifetime, vitals =>
            {
                var killed = vitals.IsKillAfter(previous);
                previous = vitals;
                if (killed)
                    handler();
            });
        }
    }
}
