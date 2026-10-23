using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Combat.Hands.Network.Codecs
{
    public struct TickedHandsSlotsCodec : ICodec<TickedState<HandsSlots>>
    {
        public TickedState<HandsSlots> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<HandsSlots> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var inHands = Value.State.InHands;
            var previous = Value.State.Previous;
            var takenByGuard = Value.State.TakenByGuard;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref inHands);
            serializer.SerializeValue(ref previous);
            serializer.SerializeValue(ref takenByGuard);

            Value = new TickedState<HandsSlots>(tick, new HandsSlots(inHands, previous, takenByGuard));
        }
    }
}
