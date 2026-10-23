using System;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsForm
    {
        private readonly WeaponInventoryConfig _config;

        public HandsControlsForm(WeaponInventoryConfig config)
        {
            _config = config;
        }

        public HandsInput InputOf(HandsControls controls, bool guarding) =>
            new(new WeaponInput(controls.Aim, controls.Trigger.Pulled, controls.ReloadPressed, guarding), ChoiceOf(controls.Choice));

        public DrawControl ControlOf(DrawChoice choice) =>
            choice.Switch(
                _config,
                static _ => DrawControl.Keep,
                static (config, slot) => DrawControl.ToSlot(IndexOf(config, slot)),
                static _ => DrawControl.Last,
                static (_, direction) => DrawControl.Scrolled(direction));

        private DrawChoice ChoiceOf(DrawControl control) =>
            control.Kind switch
            {
                DrawControlKind.Keep => DrawChoice.Keep,
                DrawControlKind.Slot => DrawChoice.ToSlot(_config.Slots[control.Value]),
                DrawControlKind.Last => DrawChoice.Last,
                DrawControlKind.Scroll => DrawChoice.Scrolled(control.Value),
                _ => throw new ArgumentOutOfRangeException(nameof(control), control.Kind, "A weapon choice keeps the hands, names a slot, the last weapon or a scroll.")
            };

        private static int IndexOf(WeaponInventoryConfig config, WeaponSlot slot)
        {
            var index = Array.IndexOf(config.Slots, slot);
            if (index < 0)
                throw new InvalidOperationException("The chosen slot is not in the inventory's layout.");

            return index;
        }
    }
}
