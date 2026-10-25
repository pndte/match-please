using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Mortality.Network.Codecs;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Mortality.Network.Codecs
{
    public sealed class GuardStateCodecTests
    {
        [TestCase(100f, false, false)]
        [TestCase(42.5f, true, false)]
        [TestCase(0f, true, true)]
        [TestCase(0f, false, true)]
        [TestCase(33.333f, true, false)]
        public void GuardStateSurvivesTheWireExactlyInFiveBytes(float health, bool raised, bool broken)
        {
            var state = new GuardState(health, raised, broken);
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new GuardStateCodec { Value = state });
            using var reader = new FastBufferReader(writer, Allocator.Temp);

            reader.ReadNetworkSerializable(out GuardStateCodec decoded);

            Assert.That(decoded.Value, Is.EqualTo(state));
            Assert.That(writer.Length, Is.EqualTo(5));
        }
    }
}
