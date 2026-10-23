using Bw.Entities.Players;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Match.Network
{
    public sealed class Combatants
    {
        private readonly IWeaponCarriers _carriers;
        private readonly ICharacterRegistry _registry;

        public Combatants(IWeaponCarriers carriers, ICharacterRegistry registry)
        {
            _carriers = carriers;
            _registry = registry;
        }

        public bool TryFindShooter(IReadonlyWeapon weapon, out IPlayer shooter)
        {
            if (_carriers.ByWeapon.TryGetValue((IWeapon)weapon, out var carrier)) //TODO: носителей ищем по пишущему IWeapon, а инициатор исхода — IReadonlyWeapon, поэтому здесь приведение типа
                return TryFindPlayer(carrier, out shooter);

            shooter = null;
            return false;
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
