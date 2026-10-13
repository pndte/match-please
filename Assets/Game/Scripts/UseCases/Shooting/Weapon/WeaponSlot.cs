using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public sealed class WeaponSlot
    {
        public WeaponKind[] Kinds = Array.Empty<WeaponKind>();
        public KeyCode Key;
    }
}
