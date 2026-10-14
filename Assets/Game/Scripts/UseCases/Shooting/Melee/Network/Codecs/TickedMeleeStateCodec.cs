using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Shooting.Melee.Network.Codecs
{
    public struct TickedMeleeStateCodec : ICodec<TickedState<MeleeState>>
    {
        public TickedState<MeleeState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<MeleeState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var aim = Value.State.Aim;
            var phase = (byte)Value.State.Phase;
            var phaseTicks = Value.State.PhaseTicks;
            var held = Value.State.Held;
            var chargeTicks = Value.State.ChargeTicks;
            var shots = Value.State.Shots;

            serializer.SerializePacked(ref tick);
            serializer.SerializeAngle(ref aim);
            serializer.SerializeValue(ref phase);
            serializer.SerializePacked(ref phaseTicks);
            serializer.SerializeValue(ref held);
            serializer.SerializePacked(ref chargeTicks);
            serializer.SerializePacked(ref shots);

            Value = new TickedState<MeleeState>(
                tick,
                new MeleeState(aim, (MeleePhase)phase, phaseTicks, held, chargeTicks, shots));
        }
    }
}
