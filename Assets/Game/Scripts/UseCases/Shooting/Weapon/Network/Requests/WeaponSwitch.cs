using System;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
{
    public readonly struct WeaponSwitch
    {
        public static readonly WeaponSwitch ToLast = new(WeaponSwitchKind.Last, 0);

        internal WeaponSwitchKind Kind { get; }
        internal int Value { get; }

        internal WeaponSwitch(WeaponSwitchKind kind, int value)
        {
            Kind = kind;
            Value = value;
        }

        public static WeaponSwitch ToSlot(int index) =>
            new(WeaponSwitchKind.Slot, index);

        public static WeaponSwitch Scrolled(int direction) =>
            new(WeaponSwitchKind.Scroll, direction);

        public void Switch<TState>(TState state, Action<TState, int> slot, Action<TState> last, Action<TState, int> scrolled)
        {
            switch (Kind)
            {
                case WeaponSwitchKind.Slot:
                    slot(state, Value);
                    break;
                case WeaponSwitchKind.Last:
                    last(state);
                    break;
                case WeaponSwitchKind.Scroll:
                    scrolled(state, Value);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"A weapon switch of kind {((byte)Kind).ToString()} is unknown: a switch goes to a slot, to the last weapon or by a scroll.");
            }
        }
    }
}
