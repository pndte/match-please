using System;
using Bw.Entities.Network.Serialization;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Tests.Entities.Network.Serialization
{
    public sealed class BufferSerializerQuantizedExtensionsTests
    {
        private const float Step = 1f / 256f;
        private const float TurnStep = 360f / 65536f;
        private const float ShareStep = 1f / 65535f;

        [TestCase(0f, 1)]
        [TestCase(15f * Step, 1)]
        [TestCase(-16f * Step, 1)]
        [TestCase(16f * Step, 2)]
        [TestCase(1f, 2)]
        [TestCase(4095f * Step, 2)]
        [TestCase(-4096f * Step, 2)]
        [TestCase(16f, 3)]
        public void FloatIsSentAsPackedSteps(float value, int bytes)
        {
            var decoded = Wire.RoundTrip(new QuantizedFloat { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(1.3f)]
        [TestCase(-27.84f)]
        [TestCase(0.001f)]
        [TestCase(1234.567f)]
        public void FloatComesBackWithinHalfAStep(float value)
        {
            var decoded = Wire.RoundTrip(new QuantizedFloat { Value = value });

            Assert.That(decoded.Value, Is.EqualTo(value).Within(Step / 2f + 1e-4f));
        }

        [TestCase(1.3f)]
        [TestCase(-27.84f)]
        public void FloatSentAgainComesBackUnchanged(float value)
        {
            var once = Wire.RoundTrip(new QuantizedFloat { Value = value });

            var twice = Wire.RoundTrip(once);

            Assert.That(twice.Value, Is.EqualTo(once.Value));
        }

        [TestCase(1e7f)]
        [TestCase(-1e7f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void FloatWithoutStepsThrows(float value)
        {
            Assert.Throws<OverflowException>(() => Wire.BytesOf(new QuantizedFloat { Value = value }));
        }

        [Test]
        public void ZeroDoubleTakesOneByte()
        {
            var decoded = Wire.RoundTrip(new QuantizedDouble { Value = 0d }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(0d));
            Assert.That(length, Is.EqualTo(1));
        }

        [TestCase(1_000_000.5d)]
        [TestCase(-123_456_789.25d)]
        [TestCase(1e12d)]
        public void DoubleKeepsLongRanges(double value)
        {
            var decoded = Wire.RoundTrip(new QuantizedDouble { Value = value });

            Assert.That(decoded.Value, Is.EqualTo(value));
        }

        [Test]
        public void DoubleComesBackWithinHalfAStep()
        {
            var decoded = Wire.RoundTrip(new QuantizedDouble { Value = 12345.678901d });

            Assert.That(decoded.Value, Is.EqualTo(12345.678901d).Within(0.5d / 65536d));
        }

        [Test]
        public void DoubleWithoutStepsThrows()
        {
            Assert.Throws<OverflowException>(() => Wire.BytesOf(new QuantizedDouble { Value = double.NaN }));
        }

        [Test]
        public void VectorIsSentAsItsTwoComponents()
        {
            var value = new Vector2(1f, -16f);

            var decoded = Wire.RoundTrip(new QuantizedVector { Value = value }, out var length);

            Assert.That(decoded.Value.x, Is.EqualTo(1f));
            Assert.That(decoded.Value.y, Is.EqualTo(-16f));
            Assert.That(length, Is.EqualTo(4));
        }

        [Test]
        public void VectorComponentsComeBackWithinHalfAStep()
        {
            var value = new Vector2(27.84f, -12.2f);

            var decoded = Wire.RoundTrip(new QuantizedVector { Value = value });

            Assert.That(decoded.Value.x, Is.EqualTo(value.x).Within(Step / 2f + 1e-5f));
            Assert.That(decoded.Value.y, Is.EqualTo(value.y).Within(Step / 2f + 1e-5f));
        }

        [TestCase(0f, 0f)]
        [TestCase(90f, 90f)]
        [TestCase(-90f, -90f)]
        [TestCase(180f, 180f)]
        [TestCase(-180f, 180f)]
        [TestCase(360f, 0f)]
        [TestCase(540f, 180f)]
        [TestCase(-450f, -90f)]
        [TestCase(359.999f, 0f)]
        public void AngleTurnsIntoTheHalfOpenTurn(float degrees, float expected)
        {
            var decoded = Wire.RoundTrip(new Angle { Degrees = degrees }, out var length);

            Assert.That(decoded.Degrees, Is.EqualTo(expected));
            Assert.That(length, Is.EqualTo(2));
        }

        [Test]
        public void AngleComesBackWithinHalfAStepAndInsideTheHalfOpenTurn()
        {
            for (var degrees = -720f; degrees <= 720f; degrees += 7.37f)
            {
                var decoded = Wire.RoundTrip(new Angle { Degrees = degrees });

                Assert.That(decoded.Degrees, Is.GreaterThan(-180f).And.LessThanOrEqualTo(180f), $"{degrees.ToString()}°");
                Assert.That(Mathf.Abs(Mathf.DeltaAngle(degrees, decoded.Degrees)), Is.LessThanOrEqualTo(TurnStep / 2f + 2e-4f), $"{degrees.ToString()}°");
            }
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void AngleWithoutADirectionThrows(float degrees)
        {
            Assert.Throws<OverflowException>(() => Wire.BytesOf(new Angle { Degrees = degrees }));
        }

        [TestCase(0f)]
        [TestCase(1f)]
        public void ShareEndsComeBackExactly(float share)
        {
            var decoded = Wire.RoundTrip(new Share { Value = share }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(share));
            Assert.That(length, Is.EqualTo(2));
        }

        [TestCase(0.5f)]
        [TestCase(0.123456f)]
        [TestCase(0.99999f)]
        [TestCase(0.00001f)]
        public void ShareComesBackWithinHalfAStep(float share)
        {
            var decoded = Wire.RoundTrip(new Share { Value = share });

            Assert.That(decoded.Value, Is.EqualTo(share).Within(ShareStep / 2f + 1e-7f));
        }

        [TestCase(1.0001f)]
        [TestCase(-0.001f)]
        [TestCase(float.NaN)]
        public void ShareOutsideTheWholeThrows(float share)
        {
            Assert.Throws<OverflowException>(() => Wire.BytesOf(new Share { Value = share }));
        }

        private struct QuantizedFloat : INetworkSerializable
        {
            private static readonly Quantization Quantization = new(8);

            public float Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeQuantized(ref Value, Quantization);
        }

        private struct QuantizedDouble : INetworkSerializable
        {
            private static readonly Quantization Quantization = new(16);

            public double Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeQuantized(ref Value, Quantization);
        }

        private struct QuantizedVector : INetworkSerializable
        {
            private static readonly Quantization Quantization = new(8);

            public Vector2 Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeQuantized(ref Value, Quantization);
        }

        private struct Angle : INetworkSerializable
        {
            public float Degrees;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeAngle(ref Degrees);
        }

        private struct Share : INetworkSerializable
        {
            public float Value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeShare(ref Value);
        }
    }
}
