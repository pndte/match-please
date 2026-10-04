using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public struct TickedMovementStateCodec : ICodec<TickedState<MovementState>>
    {
        public TickedState<MovementState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<MovementState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var position = Value.State.Position;
            var velocity = Value.State.Velocity;
            var grounded = Value.State.Grounded;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref grounded);

            Value = new TickedState<MovementState>(tick, new MovementState(position, velocity, grounded));
        }
    }
}
