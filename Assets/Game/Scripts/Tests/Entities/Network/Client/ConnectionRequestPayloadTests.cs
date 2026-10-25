using System.Text;
using Bw.Entities.Network;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ConnectionRequestPayloadTests
    {
        private const string Separator = "\u001F";

        [TestCase("Лёха", 37)]
        [TestCase("Player", 1)]
        [TestCase("Ёжик в тумане", 0)]
        [TestCase("", 4)]
        [TestCase("a", int.MaxValue)]
        public void RequestSurvivesTheRoundTrip(string nickname, int characterKey)
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest(nickname, characterKey));

            var read = ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(read, Is.True);
            Assert.That(request.Nickname, Is.EqualTo(nickname));
            Assert.That(request.CharacterKey, Is.EqualTo(characterKey));
        }

        [Test]
        public void PayloadIsTheKeyTheSeparatorAndTheNicknameInUtf8()
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest("Лёха", 37));

            Assert.That(payload, Is.EqualTo(Encoding.UTF8.GetBytes("37" + Separator + "Лёха")));
            Assert.That(payload.Length, Is.EqualTo(11));
        }

        [Test]
        public void NicknameMayContainTheSeparator()
        {
            var payload = ConnectionRequestPayload.Of(new ConnectionRequest("a" + Separator + "b", 2));

            ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(request.Nickname, Is.EqualTo("a" + Separator + "b"));
            Assert.That(request.CharacterKey, Is.EqualTo(2));
        }

        [TestCase("Сева")]
        [TestCase("")]
        [TestCase(Separator + "Сева")]
        [TestCase("x" + Separator + "Сева")]
        [TestCase("+5" + Separator + "Сева")]
        [TestCase("-5" + Separator + "Сева")]
        [TestCase(" 5" + Separator + "Сева")]
        [TestCase("5 " + Separator + "Сева")]
        [TestCase("2147483648" + Separator + "Сева")]
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
            var payload = Encoding.UTF8.GetBytes("1" + Separator + new string('a', 254));

            var read = ConnectionRequestPayload.TryRead(payload, out var request);

            Assert.That(payload.Length, Is.EqualTo(256));
            Assert.That(read, Is.True);
            Assert.That(request.Nickname, Has.Length.EqualTo(254));
        }

        [Test]
        public void PayloadOverTheLimitIsRefused()
        {
            var payload = Encoding.UTF8.GetBytes("1" + Separator + new string('a', 255));

            Assert.That(ConnectionRequestPayload.TryRead(payload, out _), Is.False);
        }

        [Test]
        public void LimitCountsBytesNotLetters()
        {
            var payload = Encoding.UTF8.GetBytes("1" + Separator + new string('ж', 128));

            Assert.That(payload.Length, Is.EqualTo(258));
            Assert.That(ConnectionRequestPayload.TryRead(payload, out _), Is.False);
        }
    }
}
