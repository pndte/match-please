using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Props.Network.Codecs
{
    public struct TickedPropStateCodec : ICodec<TickedState<PropState>>
    {
        public TickedState<PropState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<PropState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var aim = Value.State.Aim;
            var throws = Value.State.Throws;
            var triggerHeld = Value.State.TriggerHeld;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref aim); //TODO: квантование
            serializer.SerializePacked(ref throws);
            serializer.SerializeValue(ref triggerHeld);

            Value = new TickedState<PropState>(tick, new PropState(aim, throws, triggerHeld));
        }
    }
}
