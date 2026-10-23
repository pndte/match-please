using System;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public readonly struct HandsState : IEquatable<HandsState>
    {
        public static readonly HandsState Empty = new(HandsSlot.Empty, HandsSlot.Empty, false);

        public readonly HandsSlot InHands;
        public readonly HandsSlot Previous;
        public readonly bool TakenByGuard;

        public HandsState(HandsSlot inHands, HandsSlot previous, bool takenByGuard)
        {
            InHands = inHands;
            Previous = previous;
            TakenByGuard = takenByGuard;
        }

        public bool Equals(HandsState other) =>
            InHands == other.InHands && Previous == other.Previous && TakenByGuard == other.TakenByGuard;

        public override bool Equals(object obj) =>
            obj is HandsState other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(InHands, Previous, TakenByGuard);
    }
}
