using System;
using Bw.UseCases.Character;
using Unity.Netcode;

namespace Bw.CoreAdapters.Characters
{
    public static class CharacterCatalogExtensions
    {
        public static CharacterDescription DefaultCharacter(this CharacterCatalog catalog) =>
            catalog.Characters[0].Character;

        public static CharacterDescription Accepted(this CharacterCatalog catalog, int requested) =>
            catalog.TryFind(requested, out var entry) ? entry.Character : catalog.DefaultCharacter();

        public static CharacterDescription CharacterOf(this CharacterCatalog catalog, int key) =>
            catalog.TryFind(key, out var entry)
                ? entry.Character
                : throw new InvalidOperationException(
                    $"The catalog '{catalog.name}' has no character with key {key.ToString()}: the server was built with another character catalog.");

        public static NetworkObject PrefabOf(this CharacterCatalog catalog, int key) =>
            catalog.TryFind(key, out var entry)
                ? entry.Prefab
                : throw new InvalidOperationException(
                    $"The catalog '{catalog.name}' has no character with key {key.ToString()}: the connection approval accepts only catalog keys.");

        private static bool TryFind(this CharacterCatalog catalog, int key, out CharacterEntry entry)
        {
            foreach (var candidate in catalog.Characters)
            {
                if (candidate.Character.Key != key)
                    continue;

                entry = candidate;
                return true;
            }

            entry = default;
            return false;
        }
    }
}
