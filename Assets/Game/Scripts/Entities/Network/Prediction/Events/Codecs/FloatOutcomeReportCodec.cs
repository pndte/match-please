using System;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.Entities.Network.Prediction.Events.Codecs
{
    public struct FloatOutcomeReportCodec : ICodec<OutcomeReport<float>>
    {
        public OutcomeReport<float> Value
        {
            get => _value;
            set => _value = value;
        }

        private OutcomeReport<float> _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var tick = Value.Tick;
            var count = serializer.IsWriter ? Value.Affected.Count : 0;

            serializer.SerializePacked(ref tick);
            serializer.SerializePacked(ref count);

            if (serializer.IsWriter)
            {
                for (var index = 0; index < count; index++)
                {
                    var target = Value.Affected[index].Target;
                    var effect = Value.Affected[index].Effect;
                    serializer.SerializePacked(ref target);
                    serializer.SerializeValue(ref effect);
                }

                return;
            }

            var affected = count == 0 ? Array.Empty<AffectedTarget<ulong, float>>() : new AffectedTarget<ulong, float>[count];
            for (var index = 0; index < count; index++)
            {
                var target = 0UL;
                var effect = 0f;
                serializer.SerializePacked(ref target);
                serializer.SerializeValue(ref effect);
                affected[index] = new AffectedTarget<ulong, float>(target, effect);
            }

            Value = new OutcomeReport<float>(tick, affected);
        }
    }
}
