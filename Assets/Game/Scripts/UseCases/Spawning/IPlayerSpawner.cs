using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public interface ICharacterSpawner
    {
        public IReadonlyMortal SpawnCharacterFor(Lifetime lifetime, IPlayer player);
    }
}