using System;
using Bw.Entities.Network.Serialization;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Serialization
{
    public sealed class QuantizationTests
    {
        [TestCase(0, 1.0f, 1)]
        [TestCase(4, 1.0f, 16)]
        [TestCase(4, 0.5f, 8)]
        [TestCase(4, -2.25f, -36)]
        [TestCase(8, 0.0f, 0)]
        public void StepsAreTheValueTimesTwoToTheFractionBits(int fractionBits, float value, int steps)
        {
            Assert.That(new Quantization(fractionBits).StepsOf(value), Is.EqualTo(steps));
        }

        [Test]
        public void StepsRoundToTheNearestStep()
        {
            var quantization = new Quantization(2);

            Assert.That(quantization.StepsOf(0.3f), Is.EqualTo(1));
            Assert.That(quantization.StepsOf(0.4f), Is.EqualTo(2));
            Assert.That(quantization.StepsOf(-0.4f), Is.EqualTo(-2));
        }

        [TestCase(0)]
        [TestCase(6)]
        [TestCase(10)]
        public void ValueSurvivesARoundTripWithinHalfAStep(int fractionBits)
        {
            var quantization = new Quantization(fractionBits);
            var halfStep = 0.5f / (1 << fractionBits);
            foreach (var value in new[] { 0f, 0.1234f, -7.89f, 123.456f, -0.0001f })
                Assert.That(quantization.ValueOf(quantization.StepsOf(value)), Is.EqualTo(value).Within(halfStep + 1e-6f));
        }

        [Test]
        public void WholeStepsComeBackExactly()
        {
            var quantization = new Quantization(5);
            for (var steps = -1000; steps <= 1000; steps += 37)
                Assert.That(quantization.StepsOf(quantization.ValueOf(steps)), Is.EqualTo(steps));
        }

        [Test]
        public void DoubleStepsKeepLongRanges()
        {
            var quantization = new Quantization(16);

            var steps = quantization.StepsOf(1_000_000d);

            Assert.That(steps, Is.EqualTo(1_000_000L << 16));
            Assert.That(quantization.ValueOf(steps), Is.EqualTo(1_000_000d));
        }

        [Test]
        public void FloatOutsideTheIntRangeThrows()
        {
            var quantization = new Quantization(16);

            Assert.Throws<OverflowException>(() => quantization.StepsOf(1_000_000f));
        }
    }
}
