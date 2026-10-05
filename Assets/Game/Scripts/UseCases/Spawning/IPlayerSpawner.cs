using Bw.Entities.Players;
using Bw.UseCases.Character;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public interface ICharacterSpawner
    {
        public ICharacter SpawnCharacterFor(Lifetime lifetime, IPlayer player);
    }
}