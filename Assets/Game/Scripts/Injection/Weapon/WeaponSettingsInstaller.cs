using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Weapons;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View.Highlight;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Extensions;
using Bw.UseCases.Combat.Weapon.Pickup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Installers/WeaponSettings")]
    public sealed class WeaponSettingsInstaller : ScriptableObjectInstaller<WeaponSettingsInstaller>
    {
        [SerializeField] private WeaponPickupConfig _pickup;
        [SerializeField] private WeaponHighlightConfig _highlight;
        [SerializeField] private WeaponInventoryConfig _inventory;
        [SerializeField] private WeaponCatalog _catalog;

        public override void InstallBindings()
        {
            RequireOneRowOfRulesPerKind();
            RequireSlotForEveryKind();
            RequireDistinctSwitchKeys();
            WeaponCatalogValidator.Require(_catalog);
            Container.BindInstance(_pickup).AsSingle();
            Container.BindInstance(_highlight).AsSingle();
            Container.BindInstance(_inventory).AsSingle();
            Container.BindInstance(_catalog).AsSingle();
        }

        private void RequireOneRowOfRulesPerKind()
        {
            foreach (WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
            {
                var rows = 0;
                foreach (var rules in _inventory.Rules)
                    if (rules.Kind == kind)
                        rows++;

                if (rows != 1)
                    throw new InvalidOperationException(
                        $"Weapon settings '{name}' have {rows.ToString()} rows of rules for {kind.ToString()} weapons: every kind needs exactly one.");
            }
        }

        private void RequireSlotForEveryKind()
        {
            foreach (WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
            {
                var slotted = false;
                foreach (var slot in _inventory.Slots)
                    slotted |= slot.IsFor(kind);

                if (!slotted)
                    throw new InvalidOperationException(
                        $"Weapon settings '{name}' have no inventory slot for {kind.ToString()} weapons: such a weapon could never be carried.");
            }
        }

        private void RequireDistinctSwitchKeys()
        {
            var keys = new HashSet<KeyCode> { _inventory.LastKey };
            foreach (var slot in _inventory.Slots)
                if (slot.Key != KeyCode.None && !keys.Add(slot.Key))
                    throw new InvalidOperationException(
                        $"Weapon settings '{name}' bind {slot.Key.ToString()} to two weapon switches: one press would send both requests.");
        }
    }
}
