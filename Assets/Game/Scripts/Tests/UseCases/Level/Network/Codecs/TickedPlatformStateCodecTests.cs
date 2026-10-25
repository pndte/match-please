using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Level;
using Bw.UseCases.Level.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Level.Network.Codecs
{
    public sealed class TickedPlatformStateCodecTests
    {
        private const float HalfStep = 0.5f / ushort.MaxValue;

        [TestCase(0f)]
        [TestCase(1f)]
        public void RailEndsCrossTheWireExactly(float along)
        {
            var decoded = RoundTrip(new TickedState<PlatformState>(7, new PlatformState(along)), out _);

            Assert.That(decoded.State.Along, Is.EqualTo(along));
        }

        [TestCase(0.123456f)]
        [TestCase(0.5f)]
        [TestCase(0.999999f)]
        [TestCase(0.000001f)]
        public void ShareCrossesTheWireWithinHalfAStep(float along)
        {
            var decoded = RoundTrip(new TickedState<PlatformState>(7, new PlatformState(along)), out _);

            Assert.That(decoded.State.Along, Is.EqualTo(along).Within(HalfStep));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(54000)]
        [TestCase(int.MaxValue)]
        public void TickCrossesTheWireUnchanged(int tick)
        {
            var decoded = RoundTrip(new TickedState<PlatformState>(tick, new PlatformState(0.5f)), out _);

            Assert.That(decoded.Tick, Is.EqualTo(tick));
        }

        [Test]
        public void HalfAnHourOfTicksFitsInFiveBytes()
        {
            RoundTrip(new TickedState<PlatformState>(54000, new PlatformState(0.5f)), out var bytes);

            Assert.That(bytes, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void ShareOfAMillimetreOverAWholeLiftShaftIsKept()
        {
            const float shaft = 22.5f;
            var decoded = RoundTrip(new TickedState<PlatformState>(1, new PlatformState(0.3f)), out _);

            Assert.That((decoded.State.Along - 0.3f) * shaft, Is.EqualTo(0f).Within(0.0002f));
        }

        [TestCase(1.5f)]
        [TestCase(-0.1f)]
        public void ShareBeyondTheRailThrows(float along)
        {
            Assert.Throws<OverflowException>(() => RoundTrip(new TickedState<PlatformState>(1, new PlatformState(along)), out _));
        }

        private static TickedState<PlatformState> RoundTrip(TickedState<PlatformState> state, out int bytes)
        {
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedPlatformStateCodec { Value = state });
            bytes = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedPlatformStateCodec decoded);
            return decoded.Value;
        }
    }
}
