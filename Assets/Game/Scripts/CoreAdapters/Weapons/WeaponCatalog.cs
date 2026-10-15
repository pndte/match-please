using System;
using UnityEngine;

namespace Bw.CoreAdapters.Weapons
{
    [CreateAssetMenu(fileName = "WeaponCatalog", menuName = "Configs/WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject
    {
        public WeaponEntry[] Weapons = Array.Empty<WeaponEntry>();
    }
}
