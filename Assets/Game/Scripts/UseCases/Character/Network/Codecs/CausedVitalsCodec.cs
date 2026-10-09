using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events.Codecs;
using Bw.Entities.Network.Prediction.Events.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Character.Network.Codecs
{
    public struct CausedVitalsCodec : ICodec<CausedState<CharacterVitals>>
    {
        public CausedState<CharacterVitals> Value
        {
            get => _value;
            set => _value = value;
        }

        private CausedState<CharacterVitals> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var health = Value.State.Health.Current;
            var change = Value.State.Health.Change;
            var state = (byte)Value.State.State;
            var cause = Value.Cause;

            serializer.SerializeValue(ref health);
            serializer.SerializeValue(ref change);
            serializer.SerializeValue(ref state);
            serializer.SerializeCause(ref cause);

            Value = new CausedState<CharacterVitals>(new CharacterVitals(new HealthState(health, change), (CharacterState)state), cause);
        }
    }
}
