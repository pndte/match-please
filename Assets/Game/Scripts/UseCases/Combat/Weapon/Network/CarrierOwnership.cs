using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarrierOwnership
    {
        private readonly ICharacterRegistry _characterRegistry;
        private readonly IOwnerRegistry _ownerRegistry;

        public CarrierOwnership(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponCarriers carriers,
            ICharacterRegistry characterRegistry,
            IOwnerRegistry ownerRegistry)
        {
            _characterRegistry = characterRegistry;
            _ownerRegistry = ownerRegistry;

            carriers.ByWeapon.ViewForKey(lifetime, weapon, Own);
        }

        private void Own(Lifetime carriedLifetime, ICharacter carrier) =>
            _ownerRegistry.AddOwner(carriedLifetime, _characterRegistry.PlayerByCharacter[carrier]);
    }
}
