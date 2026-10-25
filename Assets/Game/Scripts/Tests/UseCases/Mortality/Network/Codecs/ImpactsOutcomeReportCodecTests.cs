using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Mortality.Network.Codecs
{
    public sealed class ImpactsOutcomeReportCodecTests
    {
        [Test]
        public void ReportKeepsItsTickAndEveryTargetInOrder()
        {
            var decoded = RoundTrip(Report());

            Assert.That(decoded.Tick, Is.EqualTo(4321));
            Assert.That(decoded.Affected.Count, Is.EqualTo(2));
            Assert.That(decoded.Affected[0].Target, Is.EqualTo(17UL));
            Assert.That(decoded.Affected[1].Target, Is.EqualTo(900000UL));
        }

        [Test]
        public void EveryImpactOfATargetSurvivesSeparately()
        {
            var decoded = RoundTrip(Report()).Affected;

            Assert.That(decoded[0].Effect.Count, Is.EqualTo(2));
            Assert.That(decoded[0].Effect[0].Hit.Damage, Is.EqualTo(12.5f));
            Assert.That(decoded[0].Effect[0].Hit.Knockback, Is.EqualTo(new Vector2(3f, -1.5f)));
            Assert.That(decoded[0].Effect[0].Contact.Point, Is.EqualTo(new Vector2(0.25f, 0.75f)));
            Assert.That(decoded[0].Effect[1].Hit.Damage, Is.EqualTo(7.25f));
            Assert.That(decoded[1].Effect.Count, Is.EqualTo(1));
            Assert.That(decoded[1].Effect[0].Contact.Point, Is.EqualTo(new Vector2(-0.5f, 1f)));
        }

        [Test]
        public void ImpactsAreQuantized()
        {
            var report = new OutcomeReport<Impacts>(1, new[]
            {
                new AffectedTarget<ulong, Impacts>(1UL, new Impacts(new Impact(new Hit(12.142858f, new Vector2(8.33f, -2.71f)), new Contact(new Vector2(0.123f, -0.456f)))))
            });

            var impact = RoundTrip(report).Affected[0].Effect[0];

            Assert.That(impact.Hit.Damage, Is.EqualTo(12.142858f).Within(0.5f / 32f));
            Assert.That(impact.Hit.Knockback.x, Is.EqualTo(8.33f).Within(0.5f / 64f));
            Assert.That(impact.Hit.Knockback.y, Is.EqualTo(-2.71f).Within(0.5f / 64f));
            Assert.That(impact.Contact.Point.x, Is.EqualTo(0.123f).Within(0.5f / 64f));
            Assert.That(impact.Contact.Point.y, Is.EqualTo(-0.456f).Within(0.5f / 64f));
        }

        [Test]
        public void ReportWithoutTargetsIsEmpty()
        {
            var decoded = RoundTrip(new OutcomeReport<Impacts>(9, new AffectedTarget<ulong, Impacts>[0]));

            Assert.That(decoded.Tick, Is.EqualTo(9));
            Assert.That(decoded.Affected, Is.Empty);
        }

        [Test]
        public void TargetWithoutImpactsKeepsNone()
        {
            var decoded = RoundTrip(new OutcomeReport<Impacts>(9, new[] { new AffectedTarget<ulong, Impacts>(3UL, default) }));

            Assert.That(decoded.Affected[0].Target, Is.EqualTo(3UL));
            Assert.That(decoded.Affected[0].Effect.Count, Is.Zero);
        }

        private static OutcomeReport<Impacts> Report() =>
            new(4321, new[]
            {
                new AffectedTarget<ulong, Impacts>(17UL, new Impacts(new[]
                {
                    new Impact(new Hit(12.5f, new Vector2(3f, -1.5f)), new Contact(new Vector2(0.25f, 0.75f))),
                    new Impact(new Hit(7.25f, new Vector2(1f, 0f)), new Contact(new Vector2(0.5f, 0.5f)))
                })),
                new AffectedTarget<ulong, Impacts>(900000UL, new Impacts(new Impact(new Hit(40f, Vector2.zero), new Contact(new Vector2(-0.5f, 1f)))))
            });

        private static OutcomeReport<Impacts> RoundTrip(OutcomeReport<Impacts> report)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new ImpactsOutcomeReportCodec { Value = report });
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out ImpactsOutcomeReportCodec decoded);
            return decoded.Value;
        }
    }
}
