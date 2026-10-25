using Bw.Tests.Doubles;
using Bw.UseCases.Movement.Abstractions;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class BodyConfigExtensionsTests
    {
        private readonly ManualClock _step = new(0.1f);
        private readonly BodyConfig _config = new() { PushDeceleration = 20f, AirPushDeceleration = 5f, PushLift = 0.5f };

        [TestCase(5f, 3f)]
        [TestCase(-5f, -3f)]
        [TestCase(1.5f, 0f)]
        [TestCase(-1.5f, 0f)]
        [TestCase(0f, 0f)]
        public void PushSlowsByTheDecelerationOfOneStepWithoutOvershooting(float push, float decelerated)
        {
            Assert.That(_config.DeceleratedPush(push, _step), Is.EqualTo(decelerated).Within(1e-5f));
        }

        [Test]
        public void GroundedPushSlowsLikeThePlainOne()
        {
            Assert.That(_config.DeceleratedPush(5f, true, _step), Is.EqualTo(3f).Within(1e-5f));
        }

        [Test]
        public void AirbornePushSlowsByTheAirDeceleration()
        {
            Assert.That(_config.DeceleratedPush(5f, false, _step), Is.EqualTo(4.5f).Within(1e-5f));
            Assert.That(_config.DeceleratedPush(-0.2f, false, _step), Is.EqualTo(0f));
        }

        [Test]
        public void LiftScalesOnlyTheVerticalPart()
        {
            Assert.That(_config.Lifted(new Vector2(3f, 4f)), Is.EqualTo(new Vector2(3f, 2f)));
        }

        private sealed class BodyConfig : IBodyConfig
        {
            public float GravityScale { get; set; } = 1f;
            public LayerMask GroundLayer { get; set; }
            public float PushDeceleration { get; set; }
            public float AirPushDeceleration { get; set; }
            public float PushLift { get; set; }
            public float ShoveForce { get; set; }
        }
    }
}
