using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.UseCases.Combat.Hands.Network.Codecs;
using Bw.UseCases.Combat.Hands.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Combat.Hands.Network
{
    public sealed class TickedHandsSlotsCodecTests
    {
        [TestCase(7, 2, HandsSlots.Empty, true, 4)]
        [TestCase(7, HandsSlots.Empty, HandsSlots.Empty, false, 4)]
        [TestCase(0, 0, 1, false, 4)]
        [TestCase(123456, 1, 0, true, 6)]
        public void SlotsSurviveTheWireAtTheirSize(
            int tick,
            sbyte inHands,
            sbyte previous,
            bool takenByGuard,
            int bytes)
        {
            var sent = new TickedState<HandsSlots>(tick, new HandsSlots(inHands, previous, takenByGuard));
            using var writer = new FastBufferWriter(64, Allocator.Temp);
            writer.WriteNetworkSerializable(new TickedHandsSlotsCodec { Value = sent });

            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out TickedHandsSlotsCodec decoded);

            Assert.That(decoded.Value.Tick, Is.EqualTo(tick));
            Assert.That(decoded.Value.State.InHands, Is.EqualTo(inHands));
            Assert.That(decoded.Value.State.Previous, Is.EqualTo(previous));
            Assert.That(decoded.Value.State.TakenByGuard, Is.EqualTo(takenByGuard));
            Assert.That(writer.Length, Is.EqualTo(bytes));
        }
    }
}
