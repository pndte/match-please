using Bw.Entities.Players;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases
{
    public interface ICharacterRegistry
    {
        public IViewableMap<IReadonlyCharacter, IPlayer> PlayerByCharacter { get; }
    }

    public class CharacterRegistry : ICharacterRegistry
    {
        public IViewableMap<IReadonlyCharacter, IPlayer> PlayerByCharacter { get; } = new ViewableMap<IReadonlyCharacter, IPlayer>();
    }
}