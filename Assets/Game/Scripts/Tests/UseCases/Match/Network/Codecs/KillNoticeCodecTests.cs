using Bw.UseCases.Match.Network.Codecs;
using Bw.UseCases.Match.Network.Requests;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;

namespace Bw.Tests.UseCases.Match.Network.Codecs
{
    public sealed class KillNoticeCodecTests
    {
        [Test]
        public void NoticeSurvivesTheWire()
        {
            var sent = new KillNotice(new PlayerTag(1, "Сева"), new PlayerTag(2, "Гошак"), 3);

            var received = RoundTrip(sent, out _);

            Assert.That(received.Killer.Id, Is.EqualTo(1));
            Assert.That(received.Killer.Nickname, Is.EqualTo("Сева"));
            Assert.That(received.Victim.Id, Is.EqualTo(2));
            Assert.That(received.Victim.Nickname, Is.EqualTo("Гошак"));
            Assert.That(received.WeaponKey, Is.EqualTo(3));
        }

        [Test]
        public void NicknamesTakeTwoBytesPerSymbolAfterTheirLength()
        {
            var sent = new KillNotice(new PlayerTag(1, "Сева"), new PlayerTag(2, "Гошак"), 3);

            RoundTrip(sent, out var length);

            Assert.That(length, Is.EqualTo(1 + 4 + 8 + 1 + 4 + 10 + 1));
        }

        [Test]
        public void LargeIdsAndKeysSurviveTheWire()
        {
            var sent = new KillNotice(new PlayerTag(int.MaxValue, "Player_One-2"), new PlayerTag(70000, "Ёжик в тумане"), -123456);

            var received = RoundTrip(sent, out _);

            Assert.That(received.Killer.Id, Is.EqualTo(int.MaxValue));
            Assert.That(received.Killer.Nickname, Is.EqualTo("Player_One-2"));
            Assert.That(received.Victim.Id, Is.EqualTo(70000));
            Assert.That(received.Victim.Nickname, Is.EqualTo("Ёжик в тумане"));
            Assert.That(received.WeaponKey, Is.EqualTo(-123456));
        }

        [Test]
        public void EmptyNicknameSurvivesTheWire()
        {
            var sent = new KillNotice(new PlayerTag(0, string.Empty), new PlayerTag(0, string.Empty), 0);

            var received = RoundTrip(sent, out _);

            Assert.That(received.Killer.Nickname, Is.Empty);
            Assert.That(received.Victim.Nickname, Is.Empty);
        }

        private static KillNotice RoundTrip(KillNotice sent, out int length)
        {
            using var writer = new FastBufferWriter(256, Allocator.Temp);
            writer.WriteNetworkSerializable(new KillNoticeCodec { Value = sent });
            length = writer.Length;
            using var reader = new FastBufferReader(writer, Allocator.Temp);
            reader.ReadNetworkSerializable(out KillNoticeCodec decoded);
            return decoded.Value;
        }
    }
}
