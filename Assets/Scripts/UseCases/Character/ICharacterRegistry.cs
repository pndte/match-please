using Bw.Entities;
using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases
{
    public interface ICharacterRegistry
    {
        public IViewableMap<ICharacter, IClient> ClientByCharacter { get; }
    }

    public class CharacterRegistry : ICharacterRegistry
    {
        public IViewableMap<ICharacter, IClient> ClientByCharacter { get; } = new ViewableMap<ICharacter, IClient>();
    }
}