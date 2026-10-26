using Bw.Entities.Network;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class ClientTests
    {
        [Test]
        public void ClientKeepsItsRequest()
        {
            var client = new Client(new ConnectionRequest("Саня", 3, System.Guid.NewGuid()));

            Assert.That(client.Request.Nickname, Is.EqualTo("Саня"));
            Assert.That(client.Request.CharacterKey, Is.EqualTo(3));
        }

        [Test]
        public void ClientIsNamedByItsNickname()
        {
            Assert.That(new Client(new ConnectionRequest("Сева", 4, System.Guid.NewGuid())).ToString(), Is.EqualTo("Сева"));
        }
    }
}
