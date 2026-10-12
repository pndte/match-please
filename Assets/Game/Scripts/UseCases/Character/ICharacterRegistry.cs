using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases
{
    public interface ICharacterRegistry
    {
        public IViewableMap<IReadonlyMortal, IPlayer> PlayerByCharacter { get; }
    }

    public class CharacterRegistry : ICharacterRegistry
    {
        public IViewableMap<IReadonlyMortal, IPlayer> PlayerByCharacter { get; } = new ViewableMap<IReadonlyMortal, IPlayer>();
    }
}