using System;
using Bw.UseCases.Combat.Abstractions.Weapons;
using UnityEngine;

namespace Bw.UseCases.Combat.Abstractions.Inventory
{
    [Serializable]
    public sealed class WeaponSlot
    {
        public WeaponKind[] Kinds = Array.Empty<WeaponKind>();
        public KeyCode Key;
    }
}
