using System;
using Bw.Entities.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Extensions
{
    public static class CharacterRegistryExtensions
    {
        public static void ForEach<V>(this IViewableMap<ICharacter, V> charactersMap, Lifetime lifetime, Action<Lifetime, ICharacter, V> handler)
        {
            charactersMap.AdviseAdd(lifetime, (character, _) =>
                charactersMap.ViewForKey(lifetime, character, (characterLifetime, value) =>
                    handler(characterLifetime, character, value)));
        }
    }
}