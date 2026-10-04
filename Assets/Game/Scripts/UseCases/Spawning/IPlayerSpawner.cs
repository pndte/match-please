using Bw.Entities.Network;
using Bw.UseCases.Character;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public interface ICharacterSpawner
    {
        public ICharacter SpawnCharacterFor(Lifetime lifetime, IClient client);
    }
}