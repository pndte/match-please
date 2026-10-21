using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Objects;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class LocalPlayerPublisher
    {
        public LocalPlayerPublisher(
            Lifetime lifetime,
            INetworkLifetimedObject networkLifetimed,
            IReadonlyPlayerDrivers drivers,
            IReadonlyPlayerIds ids,
            INetResultSender<int> localPlayer)
        {
            networkLifetimed.SpawnedLifetime.WhenAlive(lifetime, spawnedLifetime =>
                drivers.ByClient.View(spawnedLifetime, (_, client, player) =>
                    localPlayer.SendTo(client, ids.IdOf(player))));
        }
    }
}
