using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Inventory;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Hands.Extensions;
using Bw.UseCases.Combat.Weapon.Extensions;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HandsSimulator : ISimulator<HandsInput, HandsState>
    {
        private readonly IReadonlyWeaponInventory _inventory;

        public HandsSimulator(IReadonlyWeaponInventory inventory)
        {
            _inventory = inventory;
        }

        public HandsState Step(HandsState state, HandsInput input)
        {
            var settled = Settled(state);
            return input.Weapon.Guarded
                ? Chosen(Guarded(settled), input.Choice, true)
                : Released(Chosen(settled, input.Choice, false));
        }

        public HandsState Taken(HandsState state, WeaponSlot slot)
        {
            var taken = HandsSlot.Of(slot);
            return state.TakenByGuard ? new HandsState(state.InHands, taken, true) : new HandsState(taken, Remembered(state, taken), false);
        }

        private HandsState Settled(HandsState state)
        {
            if (Holds(state.InHands))
                return state;

            var next = Holds(state.Previous) ? state.Previous : FirstHeld();
            return new HandsState(next, next == state.Previous ? HandsSlot.Empty : state.Previous, state.TakenByGuard);
        }

        private HandsState Chosen(HandsState state, DrawChoice choice, bool guarding)
        {
            if (!TryFindTarget(state, choice, out var target))
                return state;

            return guarding ? new HandsState(state.InHands, target, true) : new HandsState(target, Remembered(state, target), false);
        }

        private HandsState Guarded(HandsState state)
        {
            if (state.TakenByGuard || !TryFindFists(out var fists) || state.InHands == fists)
                return state;

            return new HandsState(fists, Remembered(state, fists), true);
        }

        private HandsState Released(HandsState state)
        {
            if (!state.TakenByGuard)
                return state;

            return Holds(state.Previous) ? new HandsState(state.Previous, state.InHands, false) : new HandsState(state.InHands, HandsSlot.Empty, false);
        }

        private HandsSlot Remembered(HandsState state, HandsSlot next)
        {
            var remembered = Remembers(state.InHands) ? state.InHands : state.Previous;
            return remembered == next ? HandsSlot.Empty : remembered;
        }

        private bool Remembers(HandsSlot slot) =>
            !(_inventory.TryFindTop(slot, out var top) && _inventory.RulesOf(top).HandsOnly);

        private bool TryFindTarget(HandsState state, DrawChoice choice, out HandsSlot target)
        {
            target = choice.Switch(
                (Simulator: this, State: state),
                static _ => HandsSlot.Empty,
                static (_, slot) => HandsSlot.Of(slot),
                static context => context.State.Previous,
                static (context, direction) => context.Simulator.Scrolled(context.State.InHands, direction));
            return target != state.InHands && Holds(target);
        }

        private HandsSlot Scrolled(HandsSlot from, int direction)
        {
            var slots = _inventory.Slots;
            var start = from.Switch(
                (Inventory: _inventory, Direction: direction),
                static context => context.Direction > 0 ? -1 : context.Inventory.Slots.Count,
                static (context, slot) => context.Inventory.IndexOf(slot));
            for (var step = 1; step <= slots.Count; step++)
            {
                var slot = HandsSlot.Of(slots[((start + direction * step) % slots.Count + slots.Count) % slots.Count]);
                if (slot != from && Holds(slot))
                    return slot;
            }

            return HandsSlot.Empty;
        }

        private HandsSlot FirstHeld()
        {
            var slots = _inventory.Slots;
            for (var index = 0; index < slots.Count; index++)
                if (_inventory.TryFindTop(slots[index], out _))
                    return HandsSlot.Of(slots[index]);

            return HandsSlot.Empty;
        }

        private bool TryFindFists(out HandsSlot fists)
        {
            foreach (var (weapon, slot) in _inventory.Carried)
                if (_inventory.RulesOf(weapon).Kind == WeaponKind.Fists && !_inventory.Covered(weapon))
                {
                    fists = HandsSlot.Of(slot);
                    return true;
                }

            fists = HandsSlot.Empty;
            return false;
        }

        private bool Holds(HandsSlot slot) =>
            _inventory.TryFindTop(slot, out _);
    }
}
