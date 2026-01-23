using System;
using Bw.Entities;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Spawning;
using Cysharp.Threading.Tasks;
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
            character.State.WhenDead(lifetime, _ => RespawnCharacter(client).Forget());
        }

        private async UniTaskVoid RespawnCharacter(IClient client)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken:_selfLifetime); //TODO: лайфтайм клиента
            _characterSpawner.SpawnCharacterFor(_selfLifetime, client);
        }
    }
}