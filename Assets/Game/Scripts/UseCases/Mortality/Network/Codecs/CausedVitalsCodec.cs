using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events.Codecs;
using Bw.Entities.Network.Prediction.Events.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Mortality.Network.Codecs
{
    public struct CausedVitalsCodec : ICodec<CausedState<Vitals>>
    {
        public CausedState<Vitals> Value
        {
            get => _value;
            set => _value = value;
        }

        private CausedState<Vitals> _value;

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

            Value = new CausedState<Vitals>(new Vitals(new HealthState(health, change), (LifeState)state), cause);
        }
    }
}
