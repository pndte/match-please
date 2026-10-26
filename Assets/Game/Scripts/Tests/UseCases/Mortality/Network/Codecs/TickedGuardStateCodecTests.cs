using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Mortality.Network.Codecs
{
    public sealed class TickedGuardStateCodecTests
    {
        [TestCase(100f, false, false, 0f, 120, 120)]
        [TestCase(42.5f, true, false, 90f, 0, 7)]
        [TestCase(0f, true, true, -45f, 3, 120)]
        [TestCase(0f, false, true, 0f, 1, 0)]
        [TestCase(33.333f, true, false, 180f, 61, 6)]
        public void GuardStateSurvivesTheWire(
            float health,
            bool raised,
            bool broken,
            float aim,
            int ticksSinceBlow,
            int ticksSinceRegen)
        {
            var sent = new TickedState<GuardState>(1234, new GuardState(health, raised, broken, aim, ticksSinceBlow, ticksSinceRegen));

            var decoded = RoundTrip(sent, out _);

            Assert.That(decoded.Tick, Is.EqualTo(1234));
            Assert.That(decoded.State, Is.EqualTo(sent.State));
        }

        [Test]
        public void LoweredGuardSendsNoAimAndComesBackFacingZero()
        {
            var raised = RoundTrip(new TickedState<GuardState>(5, new GuardState(100f, true, false, 30f, 120, 120)), out var raisedLength);
            var lowered = RoundTrip(new TickedState<GuardState>(5, new GuardState(100f, false, false, 30f, 120, 120)), out var loweredLength);

            Assert.That(raised.State.Aim, Is.EqualTo(30f).Within(0.01f));
            Assert.That(lowered.State.Aim, Is.Zero);
            Assert.That(raisedLength - loweredLength, Is.EqualTo(2));
        }

        private static TickedState<GuardState> RoundTrip(TickedState<GuardState> sent, out int length)
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedGuardStateCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedGuardStateCodec decoded);
            return decoded.Value;
        }
    }
}
