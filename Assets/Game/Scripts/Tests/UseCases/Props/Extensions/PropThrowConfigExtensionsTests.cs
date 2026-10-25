using Bw.UseCases.Props;
using Bw.UseCases.Props.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Props.Extensions
{
    public sealed class PropThrowConfigExtensionsTests
    {
        private PropThrowConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<PropThrowConfig>();
            _config.Speed = 22f;
            _config.Spin = 540f;
            _config.RunCarry = 0.5f;
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(0f, 22f, 0f)]
        [TestCase(90f, 0f, 22f)]
        [TestCase(180f, -22f, 0f)]
        [TestCase(-90f, 0f, -22f)]
        public void StandingThrowFliesAtTheThrowSpeedAlongTheAim(float aim, float x, float y)
        {
            var velocity = _config.ThrowVelocity(aim, Vector2.zero);

            Assert.That(velocity.x, Is.EqualTo(x).Within(1e-4f));
            Assert.That(velocity.y, Is.EqualTo(y).Within(1e-4f));
        }

        [Test]
        public void RunningThrowCarriesAShareOfTheHorizontalSpeedOnly()
        {
            var velocity = _config.ThrowVelocity(0f, new Vector2(12.8f, 25f));

            Assert.That(velocity.x, Is.EqualTo(22f + 6.4f).Within(1e-4f));
            Assert.That(velocity.y, Is.EqualTo(0f).Within(1e-4f));
        }

        [Test]
        public void ThrowBackWhileRunningIsSlowed()
        {
            var velocity = _config.ThrowVelocity(180f, new Vector2(12.8f, 0f));

            Assert.That(velocity.x, Is.EqualTo(-22f + 6.4f).Within(1e-4f));
        }

        [Test]
        public void NoCarryIgnoresTheRun()
        {
            _config.RunCarry = 0f;

            Assert.That(_config.ThrowVelocity(0f, new Vector2(12.8f, 0f)).x, Is.EqualTo(22f).Within(1e-4f));
        }

        [Test]
        public void FullSpeedThrowSpinsAgainstItsFlightAtFullSpin()
        {
            Assert.That(_config.SpinOf(new Vector2(22f, 0f)), Is.EqualTo(-540f).Within(1e-3f));
            Assert.That(_config.SpinOf(new Vector2(-22f, 0f)), Is.EqualTo(540f).Within(1e-3f));
        }

        [Test]
        public void SlowerThrowSpinsSlower()
        {
            Assert.That(_config.SpinOf(new Vector2(-11f, 0f)), Is.EqualTo(270f).Within(1e-3f));
        }

        [Test]
        public void FasterThrowSpinsNoFasterThanTheFullSpin()
        {
            Assert.That(_config.SpinOf(new Vector2(40f, 30f)), Is.EqualTo(-540f).Within(1e-3f));
        }

        [Test]
        public void DropDoesNotSpin()
        {
            Assert.That(_config.SpinOf(Vector2.zero), Is.EqualTo(0f));
        }
    }
}
