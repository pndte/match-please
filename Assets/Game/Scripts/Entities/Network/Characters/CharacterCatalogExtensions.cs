using System;
using Unity.Netcode;

namespace Bw.Entities.Network.Characters
{
    public static class CharacterCatalogExtensions
    {
        public static string DefaultKey(this CharacterCatalog catalog) =>
            catalog.Characters[0].Key;

        public static bool Contains(this CharacterCatalog catalog, string key)
        {
            foreach (var character in catalog.Characters)
                if (character.Key == key)
                    return true;

            return false;
        }

        public static string Accepted(this CharacterCatalog catalog, string requested) =>
            catalog.Contains(requested) ? requested : catalog.DefaultKey();

        public static NetworkObject PrefabOf(this CharacterCatalog catalog, string key)
        {
            foreach (var character in catalog.Characters)
                if (character.Key == key)
                    return character.Prefab;

            throw new InvalidOperationException(
                $"Character '{key}' is not in the catalog '{catalog.name}': a player's character key comes accepted by the catalog at the connection approval.");
        }
    }
}
