using Bw.Entities;
using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.UseCases.Character.Network.Codecs
{
    public struct CharacterVitalsCodec : ICodec<CharacterVitals>
    {
        public CharacterVitals Value
        {
            get => _value;
            set => _value = value;
        }

        private CharacterVitals _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var health = Value.Health.Current;
            var change = Value.Health.Change;
            var state = (byte)Value.State;

            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref change);
            serializer.SerializeValue(ref state);

            Value = new CharacterVitals(new HealthState(health, change), (CharacterState)state);
        }
    }
}
