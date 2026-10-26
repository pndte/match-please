using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Mortality.Guarding;
using Unity.Netcode;

namespace Bw.UseCases.Mortality.Network.Codecs
{
    public struct TickedGuardStateCodec : ICodec<TickedState<GuardState>>
    {
        public TickedState<GuardState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<GuardState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var health = Value.State.Health;
            var raised = Value.State.Raised;
            var broken = Value.State.Broken;
            var aim = Value.State.Aim;
            var ticksSinceBlow = Value.State.TicksSinceBlow;
            var ticksSinceRegen = Value.State.TicksSinceRegen;

            serializer.SerializePacked(ref tick);
            serializer.SerializeFlags(ref raised, ref broken);
            serializer.SerializeValue(ref health);
            if (raised)
                serializer.SerializeAngle(ref aim);

            serializer.SerializePacked(ref ticksSinceBlow);
            serializer.SerializePacked(ref ticksSinceRegen);

            Value = new TickedState<GuardState>(
                tick,
                new GuardState(health, raised, broken, raised ? aim : 0f, ticksSinceBlow, ticksSinceRegen));
        }
    }
}
