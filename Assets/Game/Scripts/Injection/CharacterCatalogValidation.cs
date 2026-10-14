using System;
using System.Collections.Generic;
using Bw.Entities.Network.Characters;

namespace Bw.Injection
{
    public static class CharacterCatalogValidation
    {
        public static void Require(CharacterCatalog characters)
        {
            if (characters == null)
                throw new InvalidOperationException("The character catalog is not set: assign Assets/Resources/Characters/CharacterCatalog.asset.");

            if (characters.Characters.Length == 0)
                throw new InvalidOperationException($"The character catalog '{characters.name}' is empty: add at least one character, the first one is the default.");

            var keys = new HashSet<string>();
            foreach (var character in characters.Characters)
            {
                if (string.IsNullOrEmpty(character.Key) || !keys.Add(character.Key))
                    throw new InvalidOperationException(
                        $"The character catalog '{characters.name}' has an empty or repeated key '{character.Key}': every character needs a key of its own.");

                if (character.Prefab == null)
                    throw new InvalidOperationException($"Character '{character.Key}' in the catalog '{characters.name}' has no prefab.");
            }
        }
    }
}
