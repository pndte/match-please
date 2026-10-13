using System;
using Bw.UseCases.Shooting.View.Highlight;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using Bw.UseCases.Shooting.Weapon.Pickup;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Weapon
{
    [CreateAssetMenu(fileName = "WeaponSettings", menuName = "Installers/WeaponSettings")]
    public sealed class WeaponSettingsInstaller : ScriptableObjectInstaller<WeaponSettingsInstaller>
    {
        [SerializeField] private WeaponPickupConfig _pickup = new();
        [SerializeField] private WeaponHighlightConfig _highlight = new();
        [SerializeField] private WeaponInventoryConfig _inventory = new();

        public override void InstallBindings()
        {
            RequireOneRowOfRulesPerKind();
            RequireSlotForEveryKind();
            Container.BindInstance(_pickup).AsSingle();
            Container.BindInstance(_highlight).AsSingle();
            Container.BindInstance(_inventory).AsSingle();
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
    }
}
