using System;
using Bw.Entities;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
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
            characterRegistry.PlayerByCharacter.ForEach(lifetime, HandleCharacter);
        }

        private void HandleCharacter(Lifetime lifetime, ICharacter character, IPlayer player)
        {
            character.State.WhenDead(lifetime, _ => RespawnCharacter(player).Forget());
        }

        private async UniTaskVoid RespawnCharacter(IPlayer player)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken:_selfLifetime); //TODO: лайфтайм игрока
            _characterSpawner.SpawnCharacterFor(_selfLifetime, player);
        }
    }
}