using System;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [CreateAssetMenu(fileName = "WeaponCatalog", menuName = "Configs/WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        public WeaponDescription[] Weapons = Array.Empty<WeaponDescription>();
    }
}
