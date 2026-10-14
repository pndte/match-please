using Bw.Entities.Players;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Match.Network
{
    public sealed class Combatants
    {
        private readonly IWeaponInventories _inventories;
        private readonly ICharacterRegistry _registry;

        public Combatants(IWeaponInventories inventories, ICharacterRegistry registry)
        {
            _inventories = inventories;
            _registry = registry;
        }

        public bool TryFindShooter(IReadonlyWeapon weapon, out IPlayer shooter)
        {
            if (_inventories.Carriers.TryGetValue((IWeapon)weapon, out var carrier)) //TODO: носителей ищем по пишущему IWeapon, а инициатор исхода — IReadonlyWeapon, поэтому здесь приведение типа
                return TryFindPlayer(carrier, out shooter);

            shooter = null;
            return false;
        }

        public bool TryFindPlayer(IReadonlyMortal character, out IPlayer player) =>
            _registry.PlayerByCharacter.TryGetValue(character, out player);
    }
}
