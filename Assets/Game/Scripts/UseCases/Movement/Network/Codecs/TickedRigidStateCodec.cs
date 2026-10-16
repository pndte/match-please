using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public struct TickedRigidStateCodec : ICodec<TickedState<RigidState>>
    {
        public TickedState<RigidState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<RigidState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var state = Value.State;

            serializer.SerializePacked(ref tick);
            serializer.SerializeRigid(ref state);

            Value = new TickedState<RigidState>(tick, state);
        }
    }
}
