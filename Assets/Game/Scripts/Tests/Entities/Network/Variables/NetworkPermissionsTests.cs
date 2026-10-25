using Bw.Entities.Network.Variables;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network.Variables
{
    public sealed class NetworkPermissionsTests
    {
        [Test]
        public void ClientRightsIncludeTheOwnersRights()
        {
            Assert.That(NetworkPermissions.Client.HasFlag(NetworkPermissions.Owner), Is.True);
            Assert.That(NetworkPermissions.Owner.HasFlag(NetworkPermissions.Client), Is.False);
        }

        [Test]
        public void EveryoneIsTheServerAndTheClient()
        {
            Assert.That(NetworkPermissions.Everyone, Is.EqualTo(NetworkPermissions.Server | NetworkPermissions.Client));
            Assert.That(NetworkPermissions.Everyone.HasFlag(NetworkPermissions.Owner), Is.True);
        }

        [Test]
        public void ServerRightsShareNothingWithTheClients()
        {
            Assert.That(NetworkPermissions.Server & NetworkPermissions.Client, Is.EqualTo((NetworkPermissions)0));
        }
    }
}
