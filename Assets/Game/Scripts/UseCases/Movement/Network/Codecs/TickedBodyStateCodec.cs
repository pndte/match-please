using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public struct TickedBodyStateCodec : ICodec<TickedState<BodyState>>
    {
        public TickedState<BodyState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<BodyState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var body = Value.State;

            serializer.SerializePacked(ref tick);
            serializer.SerializeBody(ref body);

            Value = new TickedState<BodyState>(tick, body);
        }
    }
}
