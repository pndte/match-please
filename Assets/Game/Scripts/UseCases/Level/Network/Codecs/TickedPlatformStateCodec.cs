using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Level.Network.Codecs
{
    public struct TickedPlatformStateCodec : ICodec<TickedState<PlatformState>>
    {
        public TickedState<PlatformState> Value
        {
            get => _value;
            set => _value = value;
        }

        private TickedState<PlatformState> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var along = Value.State.Along;

            serializer.SerializePacked(ref tick);
            serializer.SerializeShare(ref along);

            Value = new TickedState<PlatformState>(tick, new PlatformState(along));
        }
    }
}
