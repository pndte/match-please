using Bw.Entities.Network;
using Bw.UseCases.Bots.Network.Codecs;
using Bw.UseCases.Bots.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Bots.Network.Codecs
{
    public sealed class BotRosterCodecsTests
    {
        [Test]
        public void TableSurvivesTheWire()
        {
            var sent = new BotTable(new[] { new BotTableEntry(3, "Пельмень", 12, 17), new BotTableEntry(70000, "Злой Тапок", 10, 14) });

            var received = RoundTrip(new BotTableCodec { Value = sent }).Value;

            Assert.That(received.Entries.Count, Is.EqualTo(2));
            Assert.That(received.Entries[0].PlayerId, Is.EqualTo(3));
            Assert.That(received.Entries[0].Nickname, Is.EqualTo("Пельмень"));
            Assert.That(received.Entries[0].DifficultyKey, Is.EqualTo(12));
            Assert.That(received.Entries[0].PersonalityKey, Is.EqualTo(17));
            Assert.That(received.Entries[1].PlayerId, Is.EqualTo(70000));
            Assert.That(received.Entries[1].Nickname, Is.EqualTo("Злой Тапок"));
        }

        [Test]
        public void EmptyTableSurvivesTheWire()
        {
            Assert.That(RoundTrip(new BotTableCodec { Value = BotTable.Empty }).Value.Entries, Is.Empty);
        }

        [Test]
        public void JoinSurvivesTheWire()
        {
            var received = RoundTrip(new RoomBotCodec { Value = new RoomBot("Кот в Мешке", 13, 16) }).Value;

            Assert.That(received.Nickname, Is.EqualTo("Кот в Мешке"));
            Assert.That(received.DifficultyKey, Is.EqualTo(13));
            Assert.That(received.PersonalityKey, Is.EqualTo(16));
        }

        [Test]
        public void PresetChangeSurvivesTheWire()
        {
            var received = RoundTrip(new BotPresetChangeCodec { Value = new BotPresetChange(9, 10, 18) }).Value;

            Assert.That(received.PlayerId, Is.EqualTo(9));
            Assert.That(received.DifficultyKey, Is.EqualTo(10));
            Assert.That(received.PersonalityKey, Is.EqualTo(18));
        }

        private static T RoundTrip<T>(T sent) where T : INetworkSerializable, new()
        {
            using var writer = new FastBufferWriter(1024, Allocator.Temp);
            writer.WriteNetworkSerializable(sent);
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out T decoded);
            return decoded;
        }
    }
}
