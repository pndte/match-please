using System;
using Bw.UseCases.Combat.Abstractions.Inventory;

namespace Bw.UseCases.Combat.Hands
{
    public abstract class DrawChoice : IEquatable<DrawChoice>
    {
        public static readonly DrawChoice Keep = new KeepChoice();
        public static readonly DrawChoice Last = new LastChoice();

        private static readonly DrawChoice ScrolledForward = new ScrollChoice(1);
        private static readonly DrawChoice ScrolledBack = new ScrollChoice(-1);

        private DrawChoice()
        {
        }

        public static DrawChoice ToSlot(WeaponSlot slot) =>
            new SlotChoice(slot ?? throw new ArgumentNullException(nameof(slot), "A slot choice names a slot of the layout."));

        public static DrawChoice Scrolled(int direction) =>
            direction switch
            {
                1 => ScrolledForward,
                -1 => ScrolledBack,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "A scroll moves one slot forward (1) or back (-1).")
            };

        public abstract TResult Switch<TState, TResult>(
            TState state,
            Func<TState, TResult> keep,
            Func<TState, WeaponSlot, TResult> slot,
            Func<TState, TResult> last,
            Func<TState, int, TResult> scrolled);

        public abstract bool Equals(DrawChoice other);

        public override bool Equals(object obj) =>
            obj is DrawChoice other && Equals(other);

        public abstract override int GetHashCode();

        private sealed class KeepChoice : DrawChoice
        {
            public override TResult Switch<TState, TResult>(
                TState state,
                Func<TState, TResult> keep,
                Func<TState, WeaponSlot, TResult> slot,
                Func<TState, TResult> last,
                Func<TState, int, TResult> scrolled) =>
                keep(state);

            public override bool Equals(DrawChoice other) =>
                other is KeepChoice;

            public override int GetHashCode() =>
                1;
        }

        private sealed class SlotChoice : DrawChoice
        {
            private readonly WeaponSlot _slot;

            public SlotChoice(WeaponSlot slot)
            {
                _slot = slot;
            }

            public override TResult Switch<TState, TResult>(
                TState state,
                Func<TState, TResult> keep,
                Func<TState, WeaponSlot, TResult> slot,
                Func<TState, TResult> last,
                Func<TState, int, TResult> scrolled) =>
                slot(state, _slot);

            public override bool Equals(DrawChoice other) =>
                other is SlotChoice choice && choice._slot == _slot;

            public override int GetHashCode() =>
                _slot.GetHashCode();
        }

        private sealed class LastChoice : DrawChoice
        {
            public override TResult Switch<TState, TResult>(
                TState state,
                Func<TState, TResult> keep,
                Func<TState, WeaponSlot, TResult> slot,
                Func<TState, TResult> last,
                Func<TState, int, TResult> scrolled) =>
                last(state);

            public override bool Equals(DrawChoice other) =>
                other is LastChoice;

            public override int GetHashCode() =>
                2;
        }

        private sealed class ScrollChoice : DrawChoice
        {
            private readonly int _direction;

            public ScrollChoice(int direction)
            {
                _direction = direction;
            }

            public override TResult Switch<TState, TResult>(
                TState state,
                Func<TState, TResult> keep,
                Func<TState, WeaponSlot, TResult> slot,
                Func<TState, TResult> last,
                Func<TState, int, TResult> scrolled) =>
                scrolled(state, _direction);

            public override bool Equals(DrawChoice other) =>
                other is ScrollChoice choice && choice._direction == _direction;

            public override int GetHashCode() =>
                3 + _direction;
        }
    }
}
