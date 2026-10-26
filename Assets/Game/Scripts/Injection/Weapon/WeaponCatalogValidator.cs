using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Weapons;

namespace Bw.Injection.Weapon
{
    public static class WeaponCatalogValidator
    {
        public static void Require(WeaponCatalog weapons)
        {
            if (weapons == null)
                throw new InvalidOperationException("The weapon catalog is not set: assign Assets/Game/Content/Weapons/WeaponCatalog.asset to WeaponSettings.");

            var keys = new HashSet<int>();
            foreach (var entry in weapons.Weapons)
            {
                if (entry.Weapon == null)
                    throw new InvalidOperationException(
                        $"The weapon catalog '{weapons.name}' has a row without a weapon: assign a weapon description from Assets/Game/Content/Weapons or remove the row.");

                if (entry.Weapon.Key == 0 || !keys.Add(entry.Weapon.Key))
                    throw new InvalidOperationException(
                        $"Weapon '{entry.Weapon.name}' is listed twice in '{weapons.name}' or has no key of its own ({entry.Weapon.Key.ToString()}): the editor issues keys on import, so open the project in the main editor.");

                if (entry.Prefab == null)
                    throw new InvalidOperationException($"Weapon '{entry.Weapon.name}' in the catalog '{weapons.name}' has no prefab: the armory creates the weapon from it.");
            }
        }
    }
}
