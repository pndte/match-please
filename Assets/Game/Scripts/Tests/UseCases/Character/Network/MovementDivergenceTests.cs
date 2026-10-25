using Bw.UseCases.Character.Network;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Character.Network
{
    public sealed class MovementDivergenceTests
    {
        private static readonly MovementState Predicted = State(new Vector2(10f, 2f), new Vector2(6f, -3f), 0.5f, true, new JumpState(false, 2, 0));

        private readonly MovementDivergence _divergence = new();

        [Test]
        public void SameStateDoesNotDiverge()
        {
            Assert.That(_divergence.Differs(Predicted, Predicted), Is.False);
        }

        [TestCase(0.009f, 0f)]
        [TestCase(0f, -0.009f)]
        [TestCase(0.006f, 0.006f)]
        public void PositionWithinACentimetreDoesNotDiverge(float x, float y)
        {
            var actual = State(new Vector2(10f + x, 2f + y), new Vector2(6f, -3f), 0.5f, true, new JumpState(false, 2, 0));

            Assert.That(_divergence.Differs(Predicted, actual), Is.False);
        }

        [TestCase(0.011f, 0f)]
        [TestCase(0f, -0.02f)]
        [TestCase(0.008f, 0.008f)]
        public void PositionBeyondACentimetreDiverges(float x, float y)
        {
            var actual = State(new Vector2(10f + x, 2f + y), new Vector2(6f, -3f), 0.5f, true, new JumpState(false, 2, 0));

            Assert.That(_divergence.Differs(Predicted, actual), Is.True);
        }

        [TestCase(0.09f, 0f, false)]
        [TestCase(0f, 0.09f, false)]
        [TestCase(0.11f, 0f, true)]
        [TestCase(0.08f, 0.08f, true)]
        public void VelocityDivergesBeyondATenthOfAMetreASecond(float x, float y, bool diverges)
        {
            var actual = State(new Vector2(10f, 2f), new Vector2(6f + x, -3f + y), 0.5f, true, new JumpState(false, 2, 0));

            Assert.That(_divergence.Differs(Predicted, actual), Is.EqualTo(diverges));
        }

        [Test]
        public void AnyOtherPushDiverges()
        {
            var actual = State(new Vector2(10f, 2f), new Vector2(6f, -3f), 0.5001f, true, new JumpState(false, 2, 0));

            Assert.That(_divergence.Differs(Predicted, actual), Is.True);
        }

        [Test]
        public void OtherGroundingDiverges()
        {
            var actual = State(new Vector2(10f, 2f), new Vector2(6f, -3f), 0.5f, false, new JumpState(false, 2, 0));

            Assert.That(_divergence.Differs(Predicted, actual), Is.True);
        }

        [TestCase(true, 2, 0)]
        [TestCase(false, 1, 0)]
        [TestCase(false, 2, 3)]
        public void OtherJumpDiverges(bool rising, int coyoteTicks, int bufferedTicks)
        {
            var actual = State(new Vector2(10f, 2f), new Vector2(6f, -3f), 0.5f, true, new JumpState(rising, coyoteTicks, bufferedTicks));

            Assert.That(_divergence.Differs(Predicted, actual), Is.True);
        }

        private static MovementState State(
            Vector2 position,
            Vector2 velocity,
            float push,
            bool grounded,
            JumpState jump) =>
            new(new BodyState(position, velocity, push, grounded), jump);
    }
}
