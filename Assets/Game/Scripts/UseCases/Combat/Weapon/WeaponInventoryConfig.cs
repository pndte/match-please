using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon
{
    [CreateAssetMenu(fileName = "WeaponInventoryConfig", menuName = "Configs/WeaponInventoryConfig")]
    public sealed class WeaponInventoryConfig : ScriptableObject
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
