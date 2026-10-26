using System;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Unity.Netcode;

namespace Bw.CoreAdapters.Weapons
{
    public static class WeaponCatalogExtensions
    {
        public static bool Contains(this WeaponCatalog catalog, WeaponDescription weapon) =>
            catalog.TryFind(weapon, out _);

        public static WeaponDescription WeaponOf(this WeaponCatalog catalog, int key)
        {
            foreach (var entry in catalog.Weapons)
                if (entry.Weapon.Key == key)
                    return entry.Weapon;

            throw new InvalidOperationException(
                $"The catalog '{catalog.name}' has no weapon with key {key.ToString()}: the server was built with another weapon catalog.");
        }

        public static NetworkObject PrefabOf(this WeaponCatalog catalog, WeaponDescription weapon) =>
            catalog.TryFind(weapon, out var entry)
                ? entry.Prefab
                : throw new InvalidOperationException($"The catalog '{catalog.name}' has no weapon '{weapon.name}': add a row with its prefab.");

        public static WeaponKind KindOf(this WeaponCatalog catalog, WeaponDescription weapon) =>
            catalog.TryFind(weapon, out var entry)
                ? entry.Kind
                : throw new InvalidOperationException($"The catalog '{catalog.name}' has no weapon '{weapon.name}': add a row with its prefab and kind.");

        private static bool TryFind(this WeaponCatalog catalog, WeaponDescription weapon, out WeaponEntry entry)
        {
            foreach (var candidate in catalog.Weapons)
            {
                if (candidate.Weapon != weapon)
                    continue;

                entry = candidate;
                return true;
            }

            entry = default;
            return false;
        }
    }
}
