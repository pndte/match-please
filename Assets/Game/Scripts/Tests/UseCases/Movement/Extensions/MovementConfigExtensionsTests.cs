using Bw.Tests.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Extensions
{
    public sealed class MovementConfigExtensionsTests
    {
        private MovementConfig _config;

        [SetUp]
        public void SetUp() =>
            _config = ScriptableObject.CreateInstance<MovementConfig>();

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(0.1f, 3)]
        [TestCase(0f, 0)]
        [TestCase(0.07f, 2)]
        [TestCase(0.04f, 1)]
        [TestCase(0.5f, 15)]
        public void CoyoteTimeIsRoundedToTicks(float seconds, int ticks)
        {
            _config.CoyoteTime = seconds;

            Assert.That(_config.CoyoteTicks(new ManualClock(1f / 30f)), Is.EqualTo(ticks));
        }

        [TestCase(0.1f, 3)]
        [TestCase(0f, 0)]
        [TestCase(0.2f, 6)]
        public void JumpBufferTimeIsRoundedToTicks(float seconds, int ticks)
        {
            _config.JumpBufferTime = seconds;

            Assert.That(_config.JumpBufferTicks(new ManualClock(1f / 30f)), Is.EqualTo(ticks));
        }

        [Test]
        public void TicksFollowTheStepDuration()
        {
            _config.CoyoteTime = 0.1f;

            Assert.That(_config.CoyoteTicks(new ManualClock(1f / 60f)), Is.EqualTo(6));
        }
    }
}
