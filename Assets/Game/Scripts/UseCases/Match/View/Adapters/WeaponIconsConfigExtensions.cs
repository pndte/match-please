using System;
using Bw.UseCases.Shooting.Weapon;
using UnityEngine;

namespace Bw.UseCases.Match.View.Adapters
{
    public static class WeaponIconsConfigExtensions
    {
        public static Sprite IconOf(this WeaponIconsConfig icons, WeaponDescription weapon)
        {
            foreach (var row in icons.Icons)
                if (row.Weapon == weapon)
                    return row.Icon;

            throw new InvalidOperationException(
                $"'{icons.name}' has no icon for weapon '{weapon.name}': the match installer checks that every catalog weapon has one.");
        }
    }
}
