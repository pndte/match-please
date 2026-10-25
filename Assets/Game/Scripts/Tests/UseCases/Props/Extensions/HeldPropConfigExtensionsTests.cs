using Bw.Tests.Doubles;
using Bw.UseCases.Props;
using Bw.UseCases.Props.Extensions;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Props.Extensions
{
    public sealed class HeldPropConfigExtensionsTests
    {
        private static readonly Vector2 Radii = new(0.76f, 0.9f);
        private static readonly Vector2 HalfSize = new(0.5f, 0.25f);

        private HeldPropConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<HeldPropConfig>();
            _config.Gap = 0.05f;
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [TestCase(0f, 1.31f, 0f)]
        [TestCase(180f, -1.31f, 0f)]
        [TestCase(90f, 0f, 1.45f)]
        [TestCase(-90f, 0f, -1.45f)]
        public void PropTouchesTheOvalTowardTheAimAcrossItsHalfLength(float aim, float x, float y)
        {
            var offset = _config.HeldOffset(aim, Radii, HalfSize);

            Assert.That(offset.x, Is.EqualTo(x).Within(1e-4f));
            Assert.That(offset.y, Is.EqualTo(y).Within(1e-4f));
        }

        [Test]
        public void DiagonalAimMeasuresTheOvalAlongTheAim()
        {
            var offset = _config.HeldOffset(45f, new Vector2(1f, 1f), HalfSize);

            Assert.That(offset.magnitude, Is.EqualTo(1.55f).Within(1e-4f));
            Assert.That(Vector2.Angle(offset, Vector2.one), Is.LessThan(0.01f));
        }

        [Test]
        public void OffsetTurnsContinuouslyThroughStraightDown()
        {
            var previous = _config.HeldOffset(-60f, Radii, HalfSize);
            var largest = 0f;
            for (var aim = -60f; aim >= -120f; aim -= 0.5f)
            {
                var offset = _config.HeldOffset(aim, Radii, HalfSize);
                largest = Mathf.Max(largest, (offset - previous).magnitude);
                previous = offset;
            }

            Assert.That(largest, Is.LessThan(0.02f));
        }

        [TestCase(0.133f, 4)]
        [TestCase(0.09f, 3)]
        [TestCase(0.11f, 4)]
        [TestCase(0.01f, 1)]
        [TestCase(0.95f, 29)]
        public void WindUpLastsTheTimeCeiledToTicksAndAtLeastOne(float seconds, int ticks)
        {
            _config.WindUpTime = seconds;

            Assert.That(_config.WindUpTicks(new ManualClock(1f / 30f)), Is.EqualTo(ticks));
        }
    }
}
