using System;
using System.Text;
using Bw.Entities.Network;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ConnectionRequestPayloadTests
    {
        private const string Separator = "\u001F";
        private const string TokenText = "0123456789abcdef0123456789abcdef";
        private const string HostKeyText = "fedcba9876543210fedcba9876543210";

        private static readonly Guid Token = Guid.ParseExact(TokenText, "N");
        private static readonly Guid HostKey = Guid.ParseExact(HostKeyText, "N");

        [TestCase("Лёха", 37)]
        [TestCase("Player", 1)]
        [TestCase("Ёжик в тумане", 0)]
        [TestCase("", 4)]
        [TestCase("a", int.MaxValue)]
        public void RequestSurvivesTheRoundTrip(string nickname, int characterKey)
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest(nickname, characterKey, Token, Guid.Empty));

            var read = ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(read, Is.True);
            Assert.That(request.Nickname, Is.EqualTo(nickname));
            Assert.That(request.CharacterKey, Is.EqualTo(characterKey));
            Assert.That(request.Token, Is.EqualTo(Token));
        }

        [Test]
        public void PayloadIsTheKeyTheTokenTheHostKeyAndTheNicknameInUtf8()
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest("Лёха", 37, Token, HostKey));

            Assert.That(payload, Is.EqualTo(Encoding.UTF8.GetBytes("37" + Separator + TokenText + Separator + HostKeyText + Separator + "Лёха")));
        }

        [Test]
        public void HostKeySurvivesTheRoundTrip()
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest("Лёха", 37, Token, HostKey));

            ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(request.HostKey, Is.EqualTo(HostKey));
            Assert.That(request.Nickname, Is.EqualTo("Лёха"));
        }

        [Test]
        public void PayloadWithoutAHostKeyIsUnread()
        {
            var payload = Encoding.UTF8.GetBytes("37" + Separator + TokenText + Separator + "Лёха");

            Assert.That(ConnectionRequestPayload.TryRead(payload, out _), Is.False);
        }

        [Test]
        public void NicknameMayContainTheSeparator()
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest("a" + Separator + "b", 2, Token, Guid.Empty));

            ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(request.Nickname, Is.EqualTo("a" + Separator + "b"));
            Assert.That(request.CharacterKey, Is.EqualTo(2));
            Assert.That(request.Token, Is.EqualTo(Token));
        }

        [TestCase("Сева")]
        [TestCase("")]
        [TestCase(Separator + TokenText + Separator + "Сева")]
        [TestCase("x" + Separator + TokenText + Separator + "Сева")]
        [TestCase("+5" + Separator + TokenText + Separator + "Сева")]
        [TestCase("-5" + Separator + TokenText + Separator + "Сева")]
        [TestCase("2147483648" + Separator + TokenText + Separator + "Сева")]
        [TestCase("5" + Separator + "Сева")]
        [TestCase("5" + Separator + "not a token" + Separator + "Сева")]
        [TestCase("5" + Separator + TokenText)]
        public void UnreadablePayloadIsRefused(string text)
        {
            var read = ConnectionRequestPayload.TryRead(Encoding.UTF8.GetBytes(text), out var request);

            Assert.That(read, Is.False);
            Assert.That(request.Nickname, Is.Null);
            Assert.That(request.CharacterKey, Is.EqualTo(0));
        }

        [Test]
        public void PayloadOfTheLimitIsRead()
        {
            var payload = Encoding.UTF8.GetBytes("1" + Separator + TokenText + Separator + HostKeyText + Separator + new string('a', 188));

            var read = ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(payload.Length, Is.EqualTo(256));
            Assert.That(read, Is.True);
            Assert.That(request.Nickname, Has.Length.EqualTo(188));
        }

        [Test]
        public void PayloadOverTheLimitIsRefused()
        {
            var payload = Encoding.UTF8.GetBytes("1" + Separator + TokenText + Separator + HostKeyText + Separator + new string('a', 189));

            Assert.That(ConnectionRequestPayload.TryRead(payload, out _), Is.False);
        }

        [Test]
        public void LimitCountsBytesNotLetters()
        {
            var payload = Encoding.UTF8.GetBytes("1" + Separator + TokenText + Separator + new string('ж', 111));

            Assert.That(payload.Length, Is.EqualTo(257));
            Assert.That(ConnectionRequestPayload.TryRead(payload, out _), Is.False);
        }
    }
}
