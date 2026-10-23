using System;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsSlotsForm : IWireForm<HandsState, HandsSlots>
    {
        private readonly WeaponInventoryConfig _config;

        public HandsSlotsForm(WeaponInventoryConfig config)
        {
            _config = config;
        }

        public HandsSlots WireOf(HandsState state) =>
            new(IndexOf(state.InHands), IndexOf(state.Previous), state.TakenByGuard);

        public HandsState ValueOf(HandsSlots wire) =>
            new(SlotOf(wire.InHands), SlotOf(wire.Previous), wire.TakenByGuard);

        private sbyte IndexOf(HandsSlot slot) =>
            slot.Switch(
                _config,
                static _ => HandsSlots.Empty,
                static (config, weaponSlot) =>
                {
                    var index = Array.IndexOf(config.Slots, weaponSlot);
                    return index >= 0 ? (sbyte)index : throw new InvalidOperationException("The hands hold a slot that is not in the inventory's layout.");
                });

        private HandsSlot SlotOf(sbyte index)
        {
            if (index == HandsSlots.Empty)
                return HandsSlot.Empty;

            if (index < 0 || index >= _config.Slots.Length)
                throw new ArgumentOutOfRangeException(nameof(index), index, $"The hands name slot {index.ToString()}, and the layout has {_config.Slots.Length.ToString()}.");

            return HandsSlot.Of(_config.Slots[index]);
        }
    }
}
