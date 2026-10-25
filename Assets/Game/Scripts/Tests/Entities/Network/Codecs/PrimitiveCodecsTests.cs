using Bw.Entities.Network.Codecs;
using Bw.Tests.Entities.Network.Doubles;
using JetBrains.Core;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Codecs
{
    public sealed class PrimitiveCodecsTests
    {
        [Test]
        public void UnitTakesNoBytes()
        {
            Assert.That(Wire.BytesOf(new UnitCodec { Value = Unit.Instance }), Is.Empty);
        }

        [TestCase(0, 1)]
        [TestCase(15, 1)]
        [TestCase(-16, 1)]
        [TestCase(16, 2)]
        [TestCase(-17, 2)]
        [TestCase(4095, 2)]
        [TestCase(-4096, 2)]
        [TestCase(4096, 3)]
        [TestCase(1048575, 3)]
        [TestCase(-1048576, 3)]
        [TestCase(1048576, 4)]
        [TestCase(268435455, 4)]
        [TestCase(-268435456, 4)]
        [TestCase(268435456, 5)]
        [TestCase(int.MaxValue, 5)]
        [TestCase(int.MinValue, 5)]
        public void IntIsPacked(int value, int bytes)
        {
            var decoded = Wire.RoundTrip(new IntCodec { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(bytes));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BoolTakesOneByte(bool value)
        {
            var decoded = Wire.RoundTrip(new BoolCodec { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(1));
        }

        [TestCase("Player")]
        [TestCase("Ёжик в тумане")]
        [TestCase("")]
        [TestCase("日本語")]
        public void StringKeepsEveryCharacterInTwoBytesAfterItsLength(string value)
        {
            var decoded = Wire.RoundTrip(new StringCodec { Value = value }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(value));
            Assert.That(length, Is.EqualTo(4 + 2 * value.Length));
        }
    }
}
