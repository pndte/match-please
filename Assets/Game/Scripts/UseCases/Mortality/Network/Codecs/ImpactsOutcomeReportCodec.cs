using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Mortality.Network.Codecs
{
    public struct ImpactsOutcomeReportCodec : ICodec<OutcomeReport<Impacts>>
    {
        private static readonly Quantization Damage = new(5);
        private static readonly Quantization Knockback = new(6);
        private static readonly Quantization Point = new(6);

        public OutcomeReport<Impacts> Value
        {
            get => _value;
            set => _value = value;
        }

        private OutcomeReport<Impacts> _value;

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
                    var impacts = Value.Affected[index].Effect;
                    var impactCount = impacts.Count;
                    serializer.SerializePacked(ref target);
                    serializer.SerializePacked(ref impactCount);
                    foreach (var impact in impacts)
                    {
                        var damage = impact.Hit.Damage;
                        var knockback = impact.Hit.Knockback;
                        var point = impact.Contact.Point;
                        Serialize(serializer, ref damage, ref knockback, ref point);
                    }
                }

                return;
            }

            var affected = count == 0 ? Array.Empty<AffectedTarget<ulong, Impacts>>() : new AffectedTarget<ulong, Impacts>[count];
            for (var index = 0; index < count; index++)
            {
                var target = 0UL;
                var impactCount = 0;
                serializer.SerializePacked(ref target);
                serializer.SerializePacked(ref impactCount);
                var impacts = new Impact[impactCount];
                for (var each = 0; each < impactCount; each++)
                {
                    var damage = 0f;
                    var knockback = Vector2.zero;
                    var point = Vector2.zero;
                    Serialize(serializer, ref damage, ref knockback, ref point);
                    impacts[each] = new Impact(new Hit(damage, knockback), new Contact(point));
                }

                affected[index] = new AffectedTarget<ulong, Impacts>(target, new Impacts(impacts));
            }

            Value = new OutcomeReport<Impacts>(tick, affected);
        }

        private static void Serialize<T>(
            BufferSerializer<T> serializer,
            ref float damage,
            ref Vector2 knockback,
            ref Vector2 point) where T : IReaderWriter
        {
            serializer.SerializeQuantized(ref damage, Damage);
            serializer.SerializeQuantized(ref knockback, Knockback);
            serializer.SerializeQuantized(ref point, Point);
        }
    }
}
