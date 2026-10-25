using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Combat.Shooting.Network.Codecs
{
    public sealed class TickedGunStateCodecTests
    {
        private const float AngleStep = 360f / 65536f;

        [Test]
        public void StateSurvivesTheWire()
        {
            var sent = new TickedState<GunState>(1234, new GunState(37.5f, 6, 25, 96, 300));

            var received = RoundTrip(sent, out _);

            Assert.That(received.Tick, Is.EqualTo(1234));
            Assert.That(received.State.Aim, Is.EqualTo(37.5f).Within(AngleStep));
            Assert.That(received.State.Ammo, Is.EqualTo(6));
            Assert.That(received.State.CooldownTicks, Is.EqualTo(25));
            Assert.That(received.State.ReloadTicks, Is.EqualTo(96));
            Assert.That(received.State.Shots, Is.EqualTo(300));
        }

        [TestCase(0f, 0f)]
        [TestCase(-90f, -90f)]
        [TestCase(179.9f, 179.9f)]
        [TestCase(190f, -170f)]
        [TestCase(-190f, 170f)]
        [TestCase(720f, 0f)]
        public void AimComesBackWrappedIntoHalfATurnEitherWay(float sent, float received)
        {
            var decoded = RoundTrip(new TickedState<GunState>(1, new GunState(sent, 0, 0, 0, 0)), out _);

            Assert.That(decoded.State.Aim, Is.EqualTo(received).Within(AngleStep));
        }

        [Test]
        public void AimIsCarriedInTheSixteenthBitOfATurn()
        {
            var aim = 7 * AngleStep;

            var decoded = RoundTrip(new TickedState<GunState>(1, new GunState(aim, 0, 0, 0, 0)), out _);

            Assert.That(decoded.State.Aim, Is.EqualTo(aim).Within(1e-5f));
        }

        [Test]
        public void SmallStateTakesSevenBytes()
        {
            RoundTrip(new TickedState<GunState>(5, new GunState(10f, 3, 0, 0, 1)), out var length);

            Assert.That(length, Is.EqualTo(7));
        }

        [Test]
        public void NegativeTickSurvivesTheWire()
        {
            var decoded = RoundTrip(new TickedState<GunState>(-3, new GunState(0f, 1, 0, 0, 0)), out _);

            Assert.That(decoded.Tick, Is.EqualTo(-3));
        }

        private static TickedState<GunState> RoundTrip(TickedState<GunState> sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedGunStateCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedGunStateCodec decoded);
            return decoded.Value;
        }
    }
}
