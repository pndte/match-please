using System;

namespace Bw.Entities.Network.Prediction.Events
{
    public readonly struct ActionId : IEquatable<ActionId>
    {
        public readonly ulong Initiator;
        public readonly int Tick;

        public ActionId(ulong initiator, int tick)
        {
            Initiator = initiator;
            Tick = tick;
        }

        public bool Equals(ActionId other) =>
            Initiator == other.Initiator && Tick == other.Tick;
    }
}
