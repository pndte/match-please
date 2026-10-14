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
            var position = Value.State.Body.Position;
            var velocity = Value.State.Body.Velocity;
            var horizontalPush = Value.State.Body.HorizontalPush;
            var grounded = Value.State.Body.Grounded;
            var rising = Value.State.Jump.Rising;
            var coyoteTicks = Value.State.Jump.CoyoteTicks;
            var bufferedTicks = Value.State.Jump.BufferedTicks;

            serializer.SerializePacked(ref tick);
            serializer.SerializeMotion(ref position, ref velocity, ref horizontalPush);
            serializer.SerializeFlags(ref grounded, ref rising);
            serializer.SerializePacked(ref coyoteTicks);
            serializer.SerializePacked(ref bufferedTicks);

            Value = new TickedState<MovementState>(
                tick,
                new MovementState(new BodyState(position, velocity, horizontalPush, grounded), new JumpState(rising, coyoteTicks, bufferedTicks)));
        }
    }
}
