using System;
using UnityEngine;

namespace Bw.UseCases.Match.View.Adapters
{
    [CreateAssetMenu(fileName = "WeaponIconsConfig", menuName = "Configs/WeaponIconsConfig")]
    public sealed class WeaponIconsConfig : ScriptableObject
    {
        public WeaponIcon[] Icons = Array.Empty<WeaponIcon>();
    }
}
