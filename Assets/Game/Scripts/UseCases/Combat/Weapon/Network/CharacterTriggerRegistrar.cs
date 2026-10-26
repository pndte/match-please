using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterTriggerRegistrar
    {
        public CharacterTriggerRegistrar(
            Lifetime lifetime,
            IReadonlyCharacter character,
            IReadonlyCharacterTrigger trigger,
            ICharacterTriggers triggers)
        {
            triggers.ByCharacter.AddLifetimed(lifetime, character, trigger);
        }
    }
}
