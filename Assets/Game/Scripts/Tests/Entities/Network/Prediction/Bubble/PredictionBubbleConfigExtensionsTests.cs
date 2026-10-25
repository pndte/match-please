using Bw.Entities.Network.Prediction.Bubble;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.Entities.Network.Prediction.Bubble
{
    public sealed class PredictionBubbleConfigExtensionsTests
    {
        private static readonly PredictionBubbleConfig Bubble = new() { EnterRadius = 4f, LeaveRadius = 8f, InputRelayMargin = 2f };

        [Test]
        public void DefaultsAreTheShippedRadii()
        {
            var config = new PredictionBubbleConfig();

            Assert.That(config.EnterRadius, Is.EqualTo(14f));
            Assert.That(config.LeaveRadius, Is.EqualTo(18f));
            Assert.That(config.InputRelayMargin, Is.EqualTo(6f));
        }

        [TestCase(0f, true)]
        [TestCase(8f, true)]
        [TestCase(9.5f, true)]
        [TestCase(10f, true)]
        [TestCase(10.01f, false)]
        [TestCase(30f, false)]
        public void InputsReachTheLeaveRadiusAndTheRelayMargin(float distance, bool needed)
        {
            Assert.That(Bubble.NeedsInputs(Vector2.zero, new Vector2(0f, distance)), Is.EqualTo(needed));
        }

        [Test]
        public void InputsAreMeasuredFromTheCenter()
        {
            var center = new Vector2(100f, -50f);

            Assert.That(Bubble.NeedsInputs(center, center + new Vector2(0f, 9.9f)), Is.True);
            Assert.That(Bubble.NeedsInputs(center, new Vector2(0f, 9.9f)), Is.False);
        }

        [Test]
        public void WithoutAMarginInputsGoExactlyToTheLeaveRadius()
        {
            var config = new PredictionBubbleConfig { EnterRadius = 4f, LeaveRadius = 8f, InputRelayMargin = 0f };

            Assert.That(config.NeedsInputs(Vector2.zero, new Vector2(8f, 0f)), Is.True);
            Assert.That(config.NeedsInputs(Vector2.zero, new Vector2(8.01f, 0f)), Is.False);
        }

        [Test]
        public void ZeroRadiusSendsInputsOnlyAtTheCenter()
        {
            var point = new PredictionBubbleConfig { EnterRadius = 0f, LeaveRadius = 0f, InputRelayMargin = 0f };

            Assert.That(point.NeedsInputs(Vector2.one, Vector2.one), Is.True);
            Assert.That(point.NeedsInputs(Vector2.one, Vector2.one + new Vector2(0.001f, 0f)), Is.False);
        }
    }
}
