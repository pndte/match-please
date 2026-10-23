using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsReceiver : ICarriedReplica
    {
        private readonly ICharacter _character;
        private readonly IWeaponCarriers _carriers;
        private readonly INetworkWeapons _weapons;
        private readonly List<(IWeapon Weapon, WeaponSlot Slot)> _listed = new();
        private readonly List<IWeapon> _ahead = new();

        private CarriedWeapons _latest = new(Array.Empty<CarriedWeapon>());

        public CarriedWeaponsReceiver(
            Lifetime lifetime,
            ICharacter character,
            IWeaponCarriers carriers,
            INetResultReceiver<CarriedWeapons> carried,
            INetworkWeapons weapons)
        {
            _character = character;
            _carriers = carriers;
            _weapons = weapons;

            carried.Received.Advise(lifetime, Remember);
            character.Mortal.State.WhenAlive(lifetime, aliveLifetime =>
            {
                aliveLifetime.OnTermination(_ahead.Clear);
                weapons.ByIds.Advise(aliveLifetime, _ => Apply());
            });
        }

        public void DropAhead(IWeapon weapon)
        {
            if (!_character.Inventory.Carried.ContainsKey(weapon))
                throw new InvalidOperationException("Only a weapon the character carries is dropped ahead of the server.");

            _ahead.Add(weapon);
            _character.Inventory.Drop(weapon);
        }

        public void Restore(IWeapon weapon)
        {
            _ahead.Remove(weapon);
            Apply();
        }

        private void Remember(CarriedWeapons carried)
        {
            _latest = carried;
            Apply();
        }

        private void Apply()
        {
            if (_character.Mortal.State.Value != LifeState.Alive || !TryResolveLatest())
                return;

            var inventory = _character.Inventory;
            _ahead.RemoveAll(weapon => !IsListed(weapon));
            foreach (var weapon in inventory.Carried.Keys.ToArray())
                if (inventory.RulesOf(weapon).Droppable && !Listed(weapon, inventory.Carried[weapon]))
                    inventory.Drop(weapon);

            foreach (var (weapon, slot) in _listed)
                if (!inventory.Carried.ContainsKey(weapon) && !_ahead.Contains(weapon))
                    Give(inventory, slot, weapon);
        }

        private bool TryResolveLatest()
        {
            _listed.Clear();
            foreach (var carried in _latest.Weapons)
            {
                if (!_weapons.ByIds.TryGetValue(carried.Weapon, out var weapon))
                    return false;

                _listed.Add((weapon, _character.Inventory.Slots[carried.Slot]));
            }

            return true;
        }

        private bool Listed(IWeapon weapon, WeaponSlot slot)
        {
            foreach (var listed in _listed)
                if (listed.Weapon == weapon && listed.Slot == slot)
                    return true;

            return false;
        }

        private bool IsListed(IWeapon weapon)
        {
            foreach (var listed in _listed)
                if (listed.Weapon == weapon)
                    return true;

            return false;
        }

        private void Give(IWeaponInventory inventory, WeaponSlot slot, IWeapon weapon)
        {
            if (_carriers.ByWeapon.TryGetValue(weapon, out var carrying))
                carrying.Inventory.Drop(weapon);

            inventory.Give(slot, weapon);
        }
    }
}
