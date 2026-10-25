using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class InventoryRig : IDisposable
    {
        public WeaponInventoryConfig Config { get; }
        public WeaponKindRegistry Kinds { get; } = new();
        public WeaponCarriers Carriers { get; } = new();
        public WeaponSlot FistsSlot => Config.Slots[0];
        public WeaponSlot GunSlot => Config.Slots[1];
        public WeaponSlot PropSlot => Config.Slots[2];

        private readonly Lifetime _lifetime;

        public InventoryRig(Lifetime lifetime)
        {
            _lifetime = lifetime;
            Config = ScriptableObject.CreateInstance<WeaponInventoryConfig>();
        }

        public FakeWeapon Weapon(WeaponKind kind)
        {
            var weapon = new FakeWeapon();
            Kinds.AddLifetimed(_lifetime, weapon, kind);
            return weapon;
        }

        public CharacterInventory InventoryOf(IReadonlyMortal mortal) =>
            new(_lifetime, mortal, Carriers, Kinds, Config);

        public void Dispose() =>
            Object.DestroyImmediate(Config);
    }
}
