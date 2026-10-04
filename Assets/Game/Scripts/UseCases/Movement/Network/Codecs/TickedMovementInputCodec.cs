using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Requests;
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
            var previousHorizontal = Value.PreviousInput.Horizontal;
            var previousJump = Value.PreviousInput.Jump;

            serializer.SerializePacked(ref tick);
            serializer.SerializeValue(ref horizontal);
            serializer.SerializeValue(ref jump);
            serializer.SerializeValue(ref previousHorizontal);
            serializer.SerializeValue(ref previousJump);

            Value = new TickedInput<MovementInput>(
                tick,
                new MovementInput(horizontal, jump),
                new MovementInput(previousHorizontal, previousJump));
        }
    }
}
