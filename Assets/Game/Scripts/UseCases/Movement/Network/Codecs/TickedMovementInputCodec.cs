using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Movement.Network.Codecs
{
    public struct TickedMovementInputCodec : ICodec<TickedInput<MovementInput>>
    {
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
            var previousHorizontal = Value.PreviousInput.Horizontal;
            var previousJump = Value.PreviousInput.Jump;
            var previousJumpHeld = Value.PreviousInput.JumpHeld;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref horizontal);
            serializer.SerializeValue(ref jump);
            serializer.SerializeValue(ref jumpHeld);
            serializer.SerializeValue(ref previousHorizontal);
            serializer.SerializeValue(ref previousJump);
            serializer.SerializeValue(ref previousJumpHeld);

            Value = new TickedInput<MovementInput>(
                tick,
                new MovementInput(horizontal, jump, jumpHeld),
                new MovementInput(previousHorizontal, previousJump, previousJumpHeld));
        }
    }
}
