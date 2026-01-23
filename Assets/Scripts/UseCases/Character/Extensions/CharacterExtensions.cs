using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Extensions
{
    public static class CharacterExtensions
    {
        public static void WhenDead(this IReadonlyProperty<CharacterState> characterState, Lifetime lifetime, Action<Lifetime> handler)
        {
            characterState.View(lifetime, (stateLifetime, state) =>
            {
                if (state == CharacterState.Dead) handler(stateLifetime);
            });
        }
    }
}