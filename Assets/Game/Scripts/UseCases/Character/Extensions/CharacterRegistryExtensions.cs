using System;
using Bw.Entities.Extensions;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Extensions
{
    public static class CharacterRegistryExtensions
    {
        public static void ForEach<V>(this IViewableMap<IReadonlyMortal, V> charactersMap, Lifetime lifetime, Action<Lifetime, IReadonlyMortal, V> handler)
        {
            charactersMap.AdviseAdd(lifetime, (character, _) =>
                charactersMap.ViewForKey(lifetime, character, (characterLifetime, value) =>
                    handler(characterLifetime, character, value)));
        }
    }
}