using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public sealed class WeaponInventoryConfig
    {
        public WeaponSlot[] Slots =
        {
            new() { Kinds = new[] { WeaponKind.Fists }, Key = KeyCode.Alpha1 },
            new() { Kinds = new[] { WeaponKind.Gun }, Key = KeyCode.Alpha2 },
            new() { Kinds = new[] { WeaponKind.Prop } }
        };

        public WeaponKindRules[] Rules =
        {
            new() { Kind = WeaponKind.Fists },
            new() { Kind = WeaponKind.Gun, Droppable = true },
            new() { Kind = WeaponKind.Prop, Droppable = true, HandsOnly = true }
        };

        public KeyCode LastKey = KeyCode.Q;
    }
}
