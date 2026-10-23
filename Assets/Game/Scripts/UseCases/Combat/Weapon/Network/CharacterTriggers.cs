using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterTriggers : ICharacterTriggers
    {
        public IViewableMap<IReadonlyCharacter, IReadonlyCharacterTrigger> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, IReadonlyCharacterTrigger>();
    }
}
