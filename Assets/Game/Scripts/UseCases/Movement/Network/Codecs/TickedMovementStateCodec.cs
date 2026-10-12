using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
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
            var body = Value.State.Body;
            var rising = Value.State.Jump.Rising;
            var coyoteTicks = Value.State.Jump.CoyoteTicks;
            var bufferedTicks = Value.State.Jump.BufferedTicks;

            serializer.SerializePacked(ref tick);
            serializer.SerializeBody(ref body);
            serializer.SerializeValue(ref rising);
            serializer.SerializePacked(ref coyoteTicks);
            serializer.SerializePacked(ref bufferedTicks);

            Value = new TickedState<MovementState>(tick, new MovementState(body, new JumpState(rising, coyoteTicks, bufferedTicks)));
        }
    }
}
