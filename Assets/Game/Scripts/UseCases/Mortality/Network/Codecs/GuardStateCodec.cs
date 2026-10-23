using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Mortality.Guarding;
using Unity.Netcode;

namespace Bw.UseCases.Mortality.Network.Codecs
{
    public struct GuardStateCodec : ICodec<GuardState>
    {
        public GuardState Value
        {
            get => _value;
            set => _value = value;
        }

        private GuardState _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var health = Value.Health;
            var raised = Value.Raised;
            var broken = Value.Broken;
            serializer.SerializeValue(ref health);
            serializer.SerializeFlags(ref raised, ref broken);
            Value = new GuardState(health, raised, broken);
        }
    }
}
