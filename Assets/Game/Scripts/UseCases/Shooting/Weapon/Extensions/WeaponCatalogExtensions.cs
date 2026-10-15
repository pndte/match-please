using System;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponCatalogExtensions
    {
        public static bool Contains(this WeaponCatalog catalog, WeaponDescription weapon) =>
            Array.IndexOf(catalog.Weapons, weapon) >= 0;

        public static WeaponDescription WeaponOf(this WeaponCatalog catalog, int key)
        {
            foreach (var weapon in catalog.Weapons)
                if (weapon.Key == key)
                    return weapon;

            throw new InvalidOperationException(
                $"The catalog '{catalog.name}' has no weapon with key {key.ToString()}: the server was built with another weapon catalog.");
        }
    }
}
