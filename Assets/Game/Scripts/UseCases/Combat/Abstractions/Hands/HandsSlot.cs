using System;
using Bw.UseCases.Combat.Abstractions.Inventory;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public abstract class HandsSlot : IEquatable<HandsSlot>
    {
        public static readonly HandsSlot Empty = new EmptySlot();

        private HandsSlot()
        {
        }

        public static HandsSlot Of(WeaponSlot slot) =>
            new LayoutSlot(slot ?? throw new ArgumentNullException(nameof(slot), "A slot of the hands is a slot of the layout; empty hands are HandsSlot.Empty."));

        public abstract TResult Switch<TState, TResult>(TState state, Func<TState, TResult> empty, Func<TState, WeaponSlot, TResult> slot);

        public abstract bool Equals(HandsSlot other);

        public override bool Equals(object obj) =>
            obj is HandsSlot other && Equals(other);

        public abstract override int GetHashCode();

        public static bool operator ==(HandsSlot left, HandsSlot right) =>
            left is null ? right is null : left.Equals(right);

        public static bool operator !=(HandsSlot left, HandsSlot right) =>
            !(left == right);

        private sealed class EmptySlot : HandsSlot
        {
            public override TResult Switch<TState, TResult>(TState state, Func<TState, TResult> empty, Func<TState, WeaponSlot, TResult> slot) =>
                empty(state);

            public override bool Equals(HandsSlot other) =>
                other is EmptySlot;

            public override int GetHashCode() =>
                0;
        }

        private sealed class LayoutSlot : HandsSlot
        {
            private readonly WeaponSlot _slot;

            public LayoutSlot(WeaponSlot slot)
            {
                _slot = slot;
            }

            public override TResult Switch<TState, TResult>(TState state, Func<TState, TResult> empty, Func<TState, WeaponSlot, TResult> slot) =>
                slot(state, _slot);

            public override bool Equals(HandsSlot other) =>
                other is LayoutSlot layout && layout._slot == _slot;

            public override int GetHashCode() =>
                _slot.GetHashCode();
        }
    }
}
