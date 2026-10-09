using Bw.Entities.Players;
using Bw.UseCases.Character;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public interface ICharacterSpawner
    {
        public IReadonlyCharacter SpawnCharacterFor(Lifetime lifetime, IPlayer player);
    }
}