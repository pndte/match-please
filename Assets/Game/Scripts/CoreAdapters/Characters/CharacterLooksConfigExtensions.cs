using System;
using Bw.UseCases.Character;

namespace Bw.CoreAdapters.Characters
{
    public static class CharacterLooksConfigExtensions
    {
        public static CharacterLook LookOf(this CharacterLooksConfig looks, CharacterDescription character)
        {
            foreach (var look in looks.Looks)
                if (look.Character == character)
                    return look;

            throw new InvalidOperationException(
                $"'{looks.name}' has no look for character '{character.name}': CharacterSettings checks that every catalog character has one.");
        }
    }
}
