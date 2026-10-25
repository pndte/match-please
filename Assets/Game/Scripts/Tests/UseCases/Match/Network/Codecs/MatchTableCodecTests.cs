using System;
using Bw.UseCases.Match;
using Bw.UseCases.Match.Network.Codecs;
using Bw.UseCases.Match.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Match.Network.Codecs
{
    public sealed class MatchTableCodecTests
    {
        [Test]
        public void EmptyTableSurvivesTheWireInThreeBytes()
        {
            var received = RoundTrip(MatchTable.Empty, out var length);

            Assert.That(received.Period, Is.EqualTo(MatchPeriod.Starting));
            Assert.That(received.Entries, Is.Empty);
            Assert.That(length, Is.EqualTo(3));
        }

        [Test]
        public void TableKeepsItsPeriodAndEntriesInOrder()
        {
            var sent = new MatchTable(new MatchPeriod(MatchPhase.Running, 18000), new[]
            {
                new MatchEntry(new PlayerTag(2, "Гошак"), 11, new Score(5, 2, 340)),
                new MatchEntry(new PlayerTag(1, "Сева"), 7, new Score(0, 4, 15))
            });

            var received = RoundTrip(sent, out _);

            Assert.That(received.Period, Is.EqualTo(new MatchPeriod(MatchPhase.Running, 18000)));
            Assert.That(received.Entries.Count, Is.EqualTo(2));
            Assert.That(received.Entries[0].Player.Id, Is.EqualTo(2));
            Assert.That(received.Entries[0].Player.Nickname, Is.EqualTo("Гошак"));
            Assert.That(received.Entries[0].CharacterKey, Is.EqualTo(11));
            Assert.That(received.Entries[0].Score.Kills, Is.EqualTo(5));
            Assert.That(received.Entries[0].Score.Deaths, Is.EqualTo(2));
            Assert.That(received.Entries[0].Score.Damage, Is.EqualTo(340));
            Assert.That(received.Entries[1].Player.Nickname, Is.EqualTo("Сева"));
            Assert.That(received.Entries[1].Score.Deaths, Is.EqualTo(4));
        }

        [TestCase(MatchPhase.Starting)]
        [TestCase(MatchPhase.Running)]
        [TestCase(MatchPhase.Ended)]
        public void EveryPhaseSurvivesTheWire(MatchPhase phase)
        {
            var received = RoundTrip(new MatchTable(new MatchPeriod(phase, 77), Array.Empty<MatchEntry>()), out _);

            Assert.That(received.Period, Is.EqualTo(new MatchPeriod(phase, 77)));
        }

        [Test]
        public void FullRoomWithLongNicknamesFitsAGrowingBuffer()
        {
            var entries = new MatchEntry[16];
            for (var index = 0; index < entries.Length; index++)
                entries[index] = new MatchEntry(new PlayerTag(index + 1, "Игрок номер " + index), index, new Score(index, index * 2, index * 100));

            var received = RoundTrip(new MatchTable(new MatchPeriod(MatchPhase.Ended, int.MaxValue), entries), out var length);

            Assert.That(received.Entries.Count, Is.EqualTo(16));
            Assert.That(received.Entries[15].Player.Nickname, Is.EqualTo("Игрок номер 15"));
            Assert.That(received.Entries[15].Score.Damage, Is.EqualTo(1500));
            Assert.That(received.Period.EndTick, Is.EqualTo(int.MaxValue));
            Assert.That(length, Is.GreaterThan(256));
        }

        [Test]
        public void TableWithAnUnknownPhaseIsRejectedOnRead()
        {
            var broken = new MatchTable(new MatchPeriod((MatchPhase)7, 0), Array.Empty<MatchEntry>());

            Assert.Throws<InvalidOperationException>(() => RoundTrip(broken, out _));
        }

        private static MatchTable RoundTrip(MatchTable sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp, ushort.MaxValue);
            writer.WriteNetworkSerializable(new MatchTableCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out MatchTableCodec decoded);
            return decoded.Value;
        }
    }
}
