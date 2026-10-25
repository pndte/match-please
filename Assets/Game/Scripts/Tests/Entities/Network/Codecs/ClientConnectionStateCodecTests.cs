using Bw.Entities;
using Bw.Entities.Network.Codecs;
using Bw.Tests.Entities.Network.Doubles;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Codecs
{
    public sealed class ClientConnectionStateCodecTests
    {
        [TestCase(ClientConnectionState.Loading)]
        [TestCase(ClientConnectionState.Connected)]
        public void StateTakesOneByte(ClientConnectionState state)
        {
            var decoded = Wire.RoundTrip(new ClientConnectionStateCodec { Value = state }, out var length);

            Assert.That(decoded.Value, Is.EqualTo(state));
            Assert.That(length, Is.EqualTo(1));
        }
    }
}
