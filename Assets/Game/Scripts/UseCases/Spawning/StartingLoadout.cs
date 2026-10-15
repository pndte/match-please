using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning
{
    public sealed class StartingLoadout
    {
        private readonly Lifetime _lifetime;
        private readonly IArmory _armory;
        private readonly IWeaponInventories _inventories;
        private readonly WeaponDescription _fists;
        private readonly WeaponDescription[] _weapons;

        public StartingLoadout(
            Lifetime lifetime,
            IArmory armory,
            IWeaponInventories inventories,
            WeaponDescription fists,
            WeaponDescription[] weapons)
        {
            _lifetime = lifetime;
            _armory = armory;
            _inventories = inventories;
            _fists = fists;
            _weapons = weapons;
        }

        public void Arm(
            IReadonlyMortal character,
            Lifetime characterLifetime,
            Vector3 position,
            Quaternion rotation)
        {
            var gun = _armory.CreateWeapon(_lifetime, RandomWeapon(), position, rotation);

            character.State.WhenAlive(characterLifetime, aliveLifetime =>
            {
                var fists = _armory.CreateWeapon(aliveLifetime, _fists, position, rotation);
                _inventories.Open(aliveLifetime, character);
                var inventory = _inventories.InventoryOf(character);
                inventory.Give(inventory.FreeSlotFor(fists), fists);
                inventory.Give(inventory.FreeSlotFor(gun), gun);
                inventory.Draw(gun);
            });
        }

        private WeaponDescription RandomWeapon() =>
            _weapons[Random.Range(0, _weapons.Length)];
    }
}
