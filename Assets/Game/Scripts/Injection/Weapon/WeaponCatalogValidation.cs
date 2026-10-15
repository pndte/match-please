using System;
using System.Collections.Generic;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Extensions;

namespace Bw.Injection.Weapon
{
    public static class WeaponCatalogValidation
    {
        public static void Require(WeaponCatalog weapons)
        {
            if (weapons == null)
                throw new InvalidOperationException("The weapon catalog is not set: assign Assets/Resources/Weapons/WeaponCatalog.asset to WeaponSettings.");

            var keys = new HashSet<int>();
            foreach (var weapon in weapons.Weapons)
            {
                if (weapon == null)
                    throw new InvalidOperationException($"The weapon catalog '{weapons.name}' has an empty row: assign a weapon description or remove the row.");

                if (weapon.Key == 0 || !keys.Add(weapon.Key))
                    throw new InvalidOperationException(
                        $"Weapon '{weapon.name}' is listed twice in '{weapons.name}' or has no key of its own ({weapon.Key.ToString()}): the editor issues keys on import, so open the project in the main editor.");
            }
        }

        public static void RequireListed(WeaponCatalog weapons, WeaponDescription description, string weapon)
        {
            if (description == null)
                throw new InvalidOperationException($"Weapon '{weapon}' has no description: assign its asset from Assets/Resources/Weapons.");

            if (!weapons.Contains(description))
                throw new InvalidOperationException(
                    $"Weapon '{weapon}' is described by '{description.name}', which the weapon catalog '{weapons.name}' lacks: add the description to the catalog.");
        }
    }
}
