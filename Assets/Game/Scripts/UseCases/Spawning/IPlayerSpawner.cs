using Bw.Entities.Players;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public interface ICharacterSpawner
    {
        public IReadonlyCharacter SpawnCharacterFor(Lifetime lifetime, IPlayer player);
    }
}
