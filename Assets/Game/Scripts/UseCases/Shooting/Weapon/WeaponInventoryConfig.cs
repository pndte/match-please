using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon
{
    [Serializable]
    public sealed class WeaponInventoryConfig
    {
        public WeaponSlot[] Slots =
        {
            new() { Kinds = new[] { WeaponKind.Fists } },
            new() { Kinds = new[] { WeaponKind.Gun } },
            new() { Kinds = new[] { WeaponKind.Prop } }
        };

        public WeaponKindRules[] Rules =
        {
            new() { Kind = WeaponKind.Fists },
            new() { Kind = WeaponKind.Gun, Droppable = true },
            new() { Kind = WeaponKind.Prop, Droppable = true, HandsOnly = true }
        };
    }
}
