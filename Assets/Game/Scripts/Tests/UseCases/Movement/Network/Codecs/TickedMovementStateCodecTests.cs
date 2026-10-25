using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Network.Codecs
{
    public sealed class TickedMovementStateCodecTests
    {
        private const float PositionStep = 1f / 256f;
        private const float VelocityStep = 1f / 64f;

        [Test]
        public void StateSurvivesTheWireWithinHalfAStep()
        {
            var sent = new MovementState(new BodyState(new Vector2(12.3456f, -7.891f), new Vector2(12.8f, -25.9187f), -3.3f, true), new JumpState(true, 2, 3));

            var received = RoundTrip(new TickedState<MovementState>(4321, sent), out _);

            Assert.That(received.Tick, Is.EqualTo(4321));
            Assert.That(received.State.Body.Position.x, Is.EqualTo(12.3456f).Within(PositionStep / 2f));
            Assert.That(received.State.Body.Position.y, Is.EqualTo(-7.891f).Within(PositionStep / 2f));
            Assert.That(received.State.Body.Velocity.x, Is.EqualTo(12.8f).Within(VelocityStep / 2f));
            Assert.That(received.State.Body.Velocity.y, Is.EqualTo(-25.9187f).Within(VelocityStep / 2f));
            Assert.That(received.State.Body.HorizontalPush, Is.EqualTo(-3.3f).Within(VelocityStep / 2f));
            Assert.That(received.State.Body.Grounded, Is.True);
            Assert.That(received.State.Jump, Is.EqualTo(new JumpState(true, 2, 3)));
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void FlagsSurviveTheWire(bool grounded, bool rising)
        {
            var sent = new MovementState(new BodyState(Vector2.zero, Vector2.zero, 0f, grounded), new JumpState(rising, 0, 0));

            var received = RoundTrip(new TickedState<MovementState>(1, sent), out _);

            Assert.That(received.State.Body.Grounded, Is.EqualTo(grounded));
            Assert.That(received.State.Jump.Rising, Is.EqualTo(rising));
        }

        [Test]
        public void StateOnTheGridComesBackExactly()
        {
            var sent = new MovementState(new BodyState(new Vector2(1.5f, -2.25f), new Vector2(0.5f, -3.5f), 0.25f, false), new JumpState(false, 0, 1));

            var received = RoundTrip(new TickedState<MovementState>(7, sent), out _);

            Assert.That(received.State, Is.EqualTo(sent));
        }

        [Test]
        public void StateAtRestTakesNineBytes()
        {
            RoundTrip(new TickedState<MovementState>(0, new MovementState(new BodyState(Vector2.zero, Vector2.zero, 0f, true), new JumpState(false, 0, 0))), out var length);

            Assert.That(length, Is.EqualTo(9));
        }

        [Test]
        public void RunningStateTakesFourteenBytes()
        {
            var running = new MovementState(new BodyState(new Vector2(1.5f, -2.25f), new Vector2(12.8f, -3.5f), 0f, true), new JumpState(false, 3, 0));

            RoundTrip(new TickedState<MovementState>(1000, running), out var length);

            Assert.That(length, Is.EqualTo(14));
        }

        private static TickedState<MovementState> RoundTrip(TickedState<MovementState> sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedMovementStateCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedMovementStateCodec decoded);
            return decoded.Value;
        }
    }
}
