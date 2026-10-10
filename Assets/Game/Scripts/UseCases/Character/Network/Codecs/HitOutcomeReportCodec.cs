using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Character.Network.Codecs
{
    public struct HitOutcomeReportCodec : ICodec<OutcomeReport<Hit>>
    {
        public OutcomeReport<Hit> Value
        {
            get => _value;
            set => _value = value;
        }

        private OutcomeReport<Hit> _value;

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
                    var damage = Value.Affected[index].Effect.Damage;
                    var knockback = Value.Affected[index].Effect.Knockback;
                    serializer.SerializePacked(ref target);
                    serializer.SerializeValue(ref damage);
                    serializer.SerializeValue(ref knockback);
                }

                return;
            }

            var affected = count == 0 ? Array.Empty<AffectedTarget<ulong, Hit>>() : new AffectedTarget<ulong, Hit>[count];
            for (var index = 0; index < count; index++)
            {
                var target = 0UL;
                var damage = 0f;
                var knockback = Vector2.zero;
                serializer.SerializePacked(ref target);
                serializer.SerializeValue(ref damage);
                serializer.SerializeValue(ref knockback);
                affected[index] = new AffectedTarget<ulong, Hit>(target, new Hit(damage, knockback));
            }

            Value = new OutcomeReport<Hit>(tick, affected);
        }
    }
}
