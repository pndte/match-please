using System;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsForm
    {
        private readonly WeaponInventoryConfig _config;
        private readonly IReadonlyHands _hands;

        public HandsControlsForm(WeaponInventoryConfig config, IReadonlyHands hands)
        {
            _config = config;
            _hands = hands;
        }

        public HandsInput InputOf(HandsControls controls, bool guarding) =>
            new(new WeaponInput(AimOf(controls.AimPoint), controls.Trigger.Pulled, controls.ReloadPressed, guarding), ChoiceOf(controls.Choice));

        public DrawControl ControlOf(DrawChoice choice) =>
            choice.Switch(
                _config,
                static _ => DrawControl.Keep,
                static (config, slot) => DrawControl.ToSlot(IndexOf(config, slot)),
                static _ => DrawControl.Last,
                static (_, direction) => DrawControl.Scrolled(direction));

        private float AimOf(Vector2 point)
        {
            var toward = point - _hands.Pose.Position;
            return Mathf.Atan2(toward.y, toward.x) * Mathf.Rad2Deg;
        }

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
