using System;

namespace Bw.UseCases.Combat.Hands.Network.Requests
{
    public readonly struct DrawControl : IEquatable<DrawControl>
    {
        public static readonly DrawControl Keep = new(DrawControlKind.Keep, 0);
        public static readonly DrawControl Last = new(DrawControlKind.Last, 0);

        public readonly DrawControlKind Kind;
        public readonly sbyte Value;

        public DrawControl(DrawControlKind kind, sbyte value)
        {
            Kind = kind;
            Value = value;
        }

        public static DrawControl ToSlot(int index) =>
            new(DrawControlKind.Slot, (sbyte)index);

        public static DrawControl Scrolled(int direction) =>
            new(DrawControlKind.Scroll, (sbyte)direction);

        public bool Equals(DrawControl other) =>
            Kind == other.Kind && Value == other.Value;

        public override bool Equals(object obj) =>
            obj is DrawControl other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(Kind, Value);
    }
}
