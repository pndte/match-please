using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public struct TickedMovementInputCodec : ICodec<TickedInput<MovementInput>>
    {
        private static readonly Quantization Axis = new(3);

        public TickedInput<MovementInput> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedInput<MovementInput> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var horizontal = Value.Input.Horizontal;
            var jump = Value.Input.Jump;
            var jumpHeld = Value.Input.JumpHeld;
            var down = Value.Input.Down;
            var previousHorizontal = Value.PreviousInput.Horizontal;
            var previousJump = Value.PreviousInput.Jump;
            var previousJumpHeld = Value.PreviousInput.JumpHeld;
            var previousDown = Value.PreviousInput.Down;

            serializer.SerializePacked(ref tick);
            serializer.SerializeQuantized(ref horizontal, Axis);
            serializer.SerializeQuantized(ref previousHorizontal, Axis);
            serializer.SerializeFlags(ref jump, ref jumpHeld, ref down, ref previousJump, ref previousJumpHeld, ref previousDown);

            Value = new TickedInput<MovementInput>(
                tick,
                new MovementInput(horizontal, jump, jumpHeld, down),
                new MovementInput(previousHorizontal, previousJump, previousJumpHeld, previousDown));
        }
    }
}
