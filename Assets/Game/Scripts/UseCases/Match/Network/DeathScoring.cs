using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class DeathScoring
    {
        private readonly ScoreKeeper _keeper;

        public DeathScoring(Lifetime lifetime, ICharacterRegistry registry, ScoreKeeper keeper)
        {
            _keeper = keeper;

            registry.PlayerByCharacter.View(lifetime, (characterLifetime, character, player) => Watch(characterLifetime, character, player));
        }

        private void Watch(Lifetime characterLifetime, IReadonlyMortal character, IPlayer player)
        {
            character.State.WhenDead(characterLifetime, _ => _keeper.CountDeath(character, player));
            characterLifetime.OnTermination(() => _keeper.Forget(character));
        }
    }
}
