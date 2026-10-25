using Bw.Tests.Doubles;
using Bw.UseCases.Props;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Props
{
    public sealed class PropSwingTests
    {
        private HeldPropConfig _config;
        private PropSwing _swing;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<HeldPropConfig>();
            _config.WindUpTime = 0.133f;
            _config.WindUpAngle = 120f;
            _swing = new PropSwing(_config, new ManualClock(1f / 30f));
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void HeldPropDoesNotSwing()
        {
            Assert.That(_swing.AngleOf(new PropState(30f, PropPhase.Holding, 3, 0, true)), Is.Zero);
        }

        [TestCase(1, 30f)]
        [TestCase(2, 60f)]
        [TestCase(3, 90f)]
        [TestCase(4, 120f)]
        public void SwingFollowsTheCurveOverTheWindUpTicks(int phaseTicks, float angle)
        {
            _config.WindUpSwing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Assert.That(_swing.AngleOf(WindingUp(0f, phaseTicks)), Is.EqualTo(angle).Within(1e-3f));
        }

        [Test]
        public void ProgressPastTheWindUpIsClamped()
        {
            _config.WindUpSwing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Assert.That(_swing.AngleOf(WindingUp(0f, 9)), Is.EqualTo(120f).Within(1e-3f));
        }

        [TestCase(10f, 1f)]
        [TestCase(-60f, 1f)]
        [TestCase(150f, -1f)]
        [TestCase(-170f, -1f)]
        public void SwingSideFollowsTheAimsSide(float aim, float side)
        {
            _config.WindUpSwing = AnimationCurve.Linear(0f, 0f, 1f, 1f);

            Assert.That(_swing.AngleOf(WindingUp(aim, 4)), Is.EqualTo(side * 120f).Within(1e-3f));
        }

        [Test]
        public void DefaultSwingGoesBackToItsPeakThenWhipsForward()
        {
            var angles = new float[4];
            for (var tick = 1; tick <= angles.Length; tick++)
                angles[tick - 1] = _swing.AngleOf(WindingUp(0f, tick));

            Assert.That(angles[0], Is.GreaterThan(0f));
            Assert.That(angles[1], Is.GreaterThan(angles[0]).And.GreaterThan(100f));
            Assert.That(angles[2], Is.LessThan(angles[1]));
            Assert.That(angles[3], Is.EqualTo(30f).Within(0.01f));
        }

        private static PropState WindingUp(float aim, int phaseTicks) =>
            new(aim, PropPhase.WindingUp, phaseTicks, 0, true);
    }
}
