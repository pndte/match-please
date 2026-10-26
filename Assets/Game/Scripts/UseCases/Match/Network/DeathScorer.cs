using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class DeathScorer
    {
        private readonly ScoreKeeper _keeper;

        public DeathScorer(Lifetime lifetime, ICharacterRegistry registry, ScoreKeeper keeper)
        {
            _keeper = keeper;

            registry.PlayerByCharacter.View(lifetime, (characterLifetime, character, _) => Watch(characterLifetime, character));
        }

        private void Watch(Lifetime characterLifetime, IReadonlyCharacter character)
        {
            character.Mortal.State.WhenDead(characterLifetime, _ => _keeper.CountDeath(character));
            characterLifetime.OnTermination(() => _keeper.Forget(character));
        }
    }
}
