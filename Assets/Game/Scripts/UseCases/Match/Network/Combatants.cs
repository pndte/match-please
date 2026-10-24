using Bw.Entities.Players;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Match.Network
{
    public sealed class Combatants
    {
        private readonly ICharacterRegistry _registry;

        public Combatants(ICharacterRegistry registry)
        {
            _registry = registry;
        }

        public bool TryFindPlayer(IReadonlyCharacter character, out IPlayer player) =>
            _registry.PlayerByCharacter.TryGetValue(character, out player);

        public bool TryFindVictim(IReadonlyMortal target, out IPlayer victim)
        {
            foreach (var (character, player) in _registry.PlayerByCharacter)
                if (character.Mortal == target)
                {
                    victim = player;
                    return true;
                }

            victim = null;
            return false;
        }
    }
}
