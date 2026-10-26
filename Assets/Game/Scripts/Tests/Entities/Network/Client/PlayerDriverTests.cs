using System;
using Bw.Entities.Network;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class PlayerDriverTests
    {
        [Test]
        public void ClientDriverTakesTheClientBranchWithItsClient()
        {
            var client = new Client(new ConnectionRequest("Саня", 3));
            IClient reached = null;
            var server = false;

            PlayerDriver.Client(client).Switch(driver => reached = driver, () => server = true);

            Assert.That(reached, Is.SameAs(client));
            Assert.That(server, Is.False);
        }

        [Test]
        public void ServerDriverTakesTheServerBranch()
        {
            var client = false;
            var server = false;

            PlayerDriver.Server.Switch(_ => client = true, () => server = true);

            Assert.That(server, Is.True);
            Assert.That(client, Is.False);
        }

        [Test]
        public void StateIsPassedToTheClientBranch()
        {
            var client = new Client(new ConnectionRequest("Сева", 4));
            var reached = "";

            PlayerDriver.Client(client).Switch(
                "client ",
                (prefix, driver) => reached = prefix + driver.Request.Nickname,
                prefix => reached = "server");

            Assert.That(reached, Is.EqualTo("client Сева"));
        }

        [Test]
        public void StateIsPassedToTheServerBranch()
        {
            var reached = 0;

            PlayerDriver.Server.Switch(41, (_, _) => reached = -1, state => reached = state + 1);

            Assert.That(reached, Is.EqualTo(42));
        }

        [Test]
        public void ClientDriverNeedsAClient()
        {
            Assert.Throws<ArgumentNullException>(() => PlayerDriver.Client(null));
        }

        [Test]
        public void ServerDriverIsOneValue()
        {
            Assert.That(PlayerDriver.Server, Is.SameAs(PlayerDriver.Server));
        }
    }
}
