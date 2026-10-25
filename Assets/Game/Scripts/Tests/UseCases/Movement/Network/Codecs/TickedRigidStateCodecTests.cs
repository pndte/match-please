using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Network.Codecs
{
    public sealed class TickedRigidStateCodecTests
    {
        private const float PositionStep = 1f / 256f;
        private const float AngleStep = 360f / 65536f;
        private const float VelocityStep = 1f / 64f;
        private const float SpinStep = 1f / 16f;

        [Test]
        public void StateSurvivesTheWireWithinHalfAStep()
        {
            var sent = new RigidState(new Vector2(-30.123f, 4.567f), 33.3f, new Vector2(7.77f, -12.34f), -540.3f);

            var received = RoundTrip(new TickedState<RigidState>(987, sent), out _);

            Assert.That(received.Tick, Is.EqualTo(987));
            Assert.That(received.State.Position.x, Is.EqualTo(-30.123f).Within(PositionStep / 2f));
            Assert.That(received.State.Position.y, Is.EqualTo(4.567f).Within(PositionStep / 2f));
            Assert.That(received.State.Angle, Is.EqualTo(33.3f).Within(AngleStep / 2f + 1e-4f));
            Assert.That(received.State.Velocity.x, Is.EqualTo(7.77f).Within(VelocityStep / 2f));
            Assert.That(received.State.Velocity.y, Is.EqualTo(-12.34f).Within(VelocityStep / 2f));
            Assert.That(received.State.AngularVelocity, Is.EqualTo(-540.3f).Within(SpinStep / 2f));
        }

        [TestCase(0f, 0f)]
        [TestCase(-45f, -45f)]
        [TestCase(270f, -90f)]
        [TestCase(-190f, 170f)]
        [TestCase(720f, 0f)]
        public void AngleComesBackWrappedIntoHalfATurnEitherWay(float sent, float received)
        {
            var decoded = RoundTrip(new TickedState<RigidState>(1, new RigidState(Vector2.zero, sent, Vector2.zero, 0f)), out _);

            Assert.That(decoded.State.Angle, Is.EqualTo(received).Within(AngleStep));
        }

        [Test]
        public void TurnedStateTakesFourteenBytes()
        {
            RoundTrip(new TickedState<RigidState>(100, new RigidState(new Vector2(1f, 2f), 30f, new Vector2(3f, -4f), 90f)), out var length);

            Assert.That(length, Is.EqualTo(14));
        }

        [Test]
        public void StateAtRestTakesEightBytes()
        {
            RoundTrip(new TickedState<RigidState>(0, new RigidState(Vector2.zero, 0f, Vector2.zero, 0f)), out var length);

            Assert.That(length, Is.EqualTo(8));
        }

        private static TickedState<RigidState> RoundTrip(TickedState<RigidState> sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedRigidStateCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedRigidStateCodec decoded);
            return decoded.Value;
        }
    }
}
