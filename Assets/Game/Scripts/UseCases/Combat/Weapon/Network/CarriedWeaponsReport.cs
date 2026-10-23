using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedWeaponsReport
    {
        public CarriedWeapons Current
        {
            get
            {
                if (_inventory.Carried.Count == 0)
                    return new CarriedWeapons(Array.Empty<CarriedWeapon>());

                var weapons = new CarriedWeapon[_inventory.Carried.Count];
                var index = 0;
                foreach (var (weapon, slot) in _inventory.Carried)
                    weapons[index++] = new CarriedWeapon(_weapons.ByIds.Inverse[weapon], (byte)_inventory.IndexOf(slot));

                return new CarriedWeapons(weapons);
            }
        }

        private readonly IReadonlyWeaponInventory _inventory;
        private readonly INetworkWeapons _weapons;

        public CarriedWeaponsReport(IReadonlyWeaponInventory inventory, INetworkWeapons weapons)
        {
            _inventory = inventory;
            _weapons = weapons;
        }
    }
}
