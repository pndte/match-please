using Bw.Entities.Players;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases
{
    public interface ICharacterRegistry
    {
        public IViewableMap<ICharacter, IPlayer> PlayerByCharacter { get; }
    }

    public class CharacterRegistry : ICharacterRegistry
    {
        public IViewableMap<ICharacter, IPlayer> PlayerByCharacter { get; } = new ViewableMap<ICharacter, IPlayer>();
    }
}