using Bw.Entities;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Spawning;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character
{
    public class CharacterRespawner
    {
        private readonly ICharacterSpawner _characterSpawner;
        private Lifetime _selfLifetime;

        public CharacterRespawner(
            Lifetime lifetime, 
            ICharacterRegistry characterRegistry,
            ICharacterSpawner characterSpawner)
        {
            _selfLifetime = lifetime;
            _characterSpawner = characterSpawner;
            characterRegistry.ClientByCharacter.ForEach(lifetime, HandleCharacter);
        }

        private void HandleCharacter(Lifetime lifetime, ICharacter character, IClient client)
        {
            character.State.WhenDead(lifetime, _ => RespawnCharacter(client));
        }

        private void RespawnCharacter(IClient client)
        {
            _characterSpawner.SpawnCharacterFor(_selfLifetime, client);
        }
    }
}