using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Props.Network.Codecs
{
    public sealed class TickedPropStateCodecTests
    {
        [Test]
        public void WindingUpStateSurvivesTheWire()
        {
            var sent = new TickedState<PropState>(1234, new PropState(47.5f, PropPhase.WindingUp, 3, 9, true));

            var received = RoundTrip(sent, out _);

            Assert.That(received.Tick, Is.EqualTo(1234));
            Assert.That(received.State.Phase, Is.EqualTo(PropPhase.WindingUp));
            Assert.That(received.State.PhaseTicks, Is.EqualTo(3));
            Assert.That(received.State.Throws, Is.EqualTo(9));
            Assert.That(received.State.TriggerHeld, Is.True);
            Assert.That(Mathf.DeltaAngle(received.State.Aim, 47.5f), Is.EqualTo(0f).Within(0.003f));
        }

        [TestCase(0f)]
        [TestCase(-90f)]
        [TestCase(179.9f)]
        [TestCase(-179.9f)]
        [TestCase(12.345f)]
        public void AimComesBackWithinHalfAnAngleStep(float aim)
        {
            var sent = new TickedState<PropState>(1, new PropState(aim, PropPhase.Holding, 0, 0, false));

            var received = RoundTrip(sent, out _);

            Assert.That(Mathf.DeltaAngle(received.State.Aim, aim), Is.EqualTo(0f).Within(360f / 65536f));
        }

        [Test]
        public void HoldingStateWithAReleasedTriggerSurvivesTheWire()
        {
            var sent = new TickedState<PropState>(0, new PropState(0f, PropPhase.Holding, 0, 0, false));

            var received = RoundTrip(sent, out _);

            Assert.That(received.State.Equals(sent.State), Is.True);
            Assert.That(received.Tick, Is.Zero);
        }

        [Test]
        public void StateNearTheStartOfASessionTakesEightBytes()
        {
            var sent = new TickedState<PropState>(100, new PropState(10f, PropPhase.WindingUp, 2, 5, true));

            RoundTrip(sent, out var length);

            Assert.That(length, Is.EqualTo(8));
        }

        private static TickedState<PropState> RoundTrip(TickedState<PropState> sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedPropStateCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedPropStateCodec decoded);
            return decoded.Value;
        }
    }
}
