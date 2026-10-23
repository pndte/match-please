namespace Bw.UseCases.Combat.Hands.Network.Requests
{
    public readonly struct HandsSlots
    {
        public const sbyte Empty = -1;

        public readonly sbyte InHands;
        public readonly sbyte Previous;
        public readonly bool TakenByGuard;

        public HandsSlots(sbyte inHands, sbyte previous, bool takenByGuard)
        {
            InHands = inHands;
            Previous = previous;
            TakenByGuard = takenByGuard;
        }
    }
}
