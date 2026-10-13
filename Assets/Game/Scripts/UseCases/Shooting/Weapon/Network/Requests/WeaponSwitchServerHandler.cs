using System;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public sealed class WeaponSwitchServerHandler
    {
        private readonly IReadonlyMortal _character;
        private readonly IWeaponInventories _inventories;
        private readonly Action<IWeaponInventory, int> _toSlot;
        private readonly Action<IWeaponInventory> _toLast;
        private readonly Action<IWeaponInventory, int> _scrolled;

        public WeaponSwitchServerHandler(
            Lifetime lifetime,
            IReadonlyMortal character,
            INetRequestReceiver<WeaponSwitch> request,
            IWeaponInventories inventories)
        {
            _character = character;
            _inventories = inventories;
            _toSlot = SwitchToSlot;
            _toLast = SwitchToLast;
            _scrolled = SwitchScrolled;

            character.State.WhenAlive(lifetime, aliveLifetime =>
                request.Received.Advise(aliveLifetime, Switch));
        }

        private void Switch(WeaponSwitch request) =>
            request.Switch(_inventories.InventoryOf(_character), _toSlot, _toLast, _scrolled);

        private static void SwitchToSlot(IWeaponInventory inventory, int index)
        {
            if (inventory.TryFindTop(inventory.Slots[index], out var weapon))
                SwitchIfDrawable(inventory, weapon);
        }

        private static void SwitchToLast(IWeaponInventory inventory)
        {
            if (inventory.TryFindLast(out var weapon))
                SwitchIfDrawable(inventory, weapon);
        }

        private static void SwitchScrolled(IWeaponInventory inventory, int direction)
        {
            if (inventory.TryFindScrolled(direction, out var weapon))
                SwitchIfDrawable(inventory, weapon);
        }

        private static void SwitchIfDrawable(IWeaponInventory inventory, IWeapon weapon)
        {
            if (inventory.Drawable(weapon))
                inventory.SwitchTo(weapon);
        }
    }
}
