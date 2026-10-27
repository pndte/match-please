using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Extensions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Bots
{
    public sealed class BotInventory
    {
        public bool GunLoaded
        {
            get
            {
                foreach (var (carried, _) in _inventory.Carried)
                    if (_inventory.RulesOf(carried).Kind == WeaponKind.Gun && IsLoaded(carried))
                        return true;

                return false;
            }
        }

        private readonly IReadonlyWeaponInventory _inventory;
        private readonly IHands _hands;
        private readonly IBotGamepad _gamepad;
        private readonly IReadonlyWeaponStates<GunState> _guns;

        public BotInventory(
            IReadonlyWeaponInventory inventory,
            IHands hands,
            IBotGamepad gamepad,
            IReadonlyWeaponStates<GunState> guns)
        {
            _inventory = inventory;
            _hands = hands;
            _gamepad = gamepad;
            _guns = guns;
        }

        public bool IsDrawn(WeaponKind kind) =>
            _hands.TryFindInHands(out var inHands) && _inventory.RulesOf(inHands).Kind == kind;

        public bool TryFindDrawable(WeaponKind kind, out IWeapon drawable)
        {
            if (!_hands.Busy)
                foreach (var (carried, slot) in _inventory.Carried)
                    if (_inventory.RulesOf(carried).Kind == kind && !_inventory.Covered(carried) && HandsSlot.Of(slot) != _hands.State.Value.InHands)
                    {
                        drawable = carried;
                        return true;
                    }

            drawable = default;
            return false;
        }

        public void Draw(IWeapon weapon) =>
            _gamepad.Choose(DrawChoice.ToSlot(_inventory.Carried[weapon]));

        public bool TryPickUp(IWeapon weapon)
        {
            if (!_hands.FreeFor(weapon))
                return false;

            _hands.Take(weapon);
            return true;
        }

        private bool IsLoaded(IWeapon gun)
        {
            var state = _guns.ByWeapon[gun].State.Value;
            return state.ReloadTicks == 0 && (state.Ammo > 0 || _hands.TryFindInHands(out var inHands) && inHands == gun);
        }
    }
}
