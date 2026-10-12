using System;
using Bw.Entities;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Character.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Spawning
{
    public class CharacterRespawner
    {
        private readonly ICharacterSpawner _characterSpawner;

        public CharacterRespawner(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry characterRegistry,
            ICharacterSpawner characterSpawner)
        {
            _characterSpawner = characterSpawner;

            players.View(lifetime, (playerLifetime, player) =>
                characterRegistry.PlayerByCharacter.ForEach(playerLifetime, (characterLifetime, character, owner) =>
                {
                    if (owner == player)
                        character.State.WhenDead(characterLifetime, _ => Respawn(playerLifetime, player).Forget());
                }));
        }

        private async UniTaskVoid Respawn(Lifetime playerLifetime, IPlayer player)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3), cancellationToken: playerLifetime);
            _characterSpawner.SpawnCharacterFor(playerLifetime, player);
        }
    }
}
