using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Weapons;
using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class Armory : IArmory, IReadonlyWeaponDescriptionRegistry
    {
        private readonly Dictionary<IReadonlyWeapon, WeaponDescription> _issued = new();
        private readonly WeaponCatalog _catalog;
        private readonly DiContainer _container;
        private int _issuedCount;

        public Armory(WeaponCatalog catalog, DiContainer container)
        {
            _catalog = catalog;
            _container = container;
        }

        public IWeapon CreateWeapon(
            Lifetime lifetime,
            WeaponDescription weapon,
            Vector3 position,
            Quaternion rotation)
        {
            var weaponObject = NetworkPrefabInstantiationHelper.Instantiate(_container, _catalog.PrefabOf(weapon), position, rotation);
            var created = HolderOf(weaponObject).Value;
            weaponObject.name += $", {_issuedCount++}";
            weaponObject.Spawn(destroyWithScene: true);
            _issued.Add(created, weapon);
            lifetime.Intersect(weaponObject.gameObject.Lifetime()).OnTermination(() =>
            {
                _issued.Remove(created);
                if (weaponObject.IsSpawned)
                    weaponObject.Despawn();
            });
            return created;
        }

        public WeaponDescription DescriptionOf(IReadonlyWeapon weapon) =>
            _issued.TryGetValue(weapon, out var description)
                ? description
                : throw new InvalidOperationException("The weapon has no description: only weapons the armory created have one, and only until they are gone.");

        private static WeaponHolder HolderOf(NetworkObject weaponObject) =>
            weaponObject.TryGetComponent(out WeaponHolder holder)
                ? holder
                : throw new InvalidOperationException($"'{weaponObject.name}' has no WeaponHolder after its prefab install: the weapon catalog lists a prefab that is not a weapon.");
    }
}
