using Bw.UseCases.Character;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Players
{
    public interface IPlayer
    {
        public IReadonlyProperty<ICharacter> ActiveCharacter { get; }
    }

    public class Player : IPlayer
    {
        public IReadonlyProperty<ICharacter> ActiveCharacter => _activeCharacter;
        private readonly ViewableProperty<ICharacter> _activeCharacter = new();
    }
}