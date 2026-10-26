using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;
using Unity.Netcode;

namespace Bw.Tests.Entities.Network.Serialization
{
    public sealed class CodecRoundTripTests
    {
        [TestCase(30.1234f)]
        [TestCase(-77.777f)]
        [TestCase(180f)]
        public void ReceivedValueIsWhatTheWireDelivers(float degrees)
        {
            var received = CodecRoundTrip.Received<float, AngleCodec>(degrees);

            Assert.That(received, Is.EqualTo(Wire.RoundTrip(new AngleCodec { Value = degrees }).Value));
        }

        [TestCase(30.1234f)]
        [TestCase(-77.777f)]
        public void ReceivedValueCrossesTheWireUnchanged(float degrees)
        {
            var received = CodecRoundTrip.Received<float, AngleCodec>(degrees);

            Assert.That(CodecRoundTrip.Received<float, AngleCodec>(received), Is.EqualTo(received));
        }

        private struct AngleCodec : ICodec<float>
        {
            public float Value
            {
                get => _value;
                set => _value = value;
            }

            private float _value;

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter =>
                serializer.SerializeAngle(ref _value);
        }
    }
}
