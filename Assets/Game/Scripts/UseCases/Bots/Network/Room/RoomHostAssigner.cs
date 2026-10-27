using Bw.Entities;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class RoomHostAssigner
    {
        public RoomHostAssigner(
            Lifetime lifetime,
            RoomSettings room,
            IReadonlyPlayerDrivers drivers,
            IOwnerRegistry owners)
        {
            drivers.ByClient.View(lifetime, (clientLifetime, client, player) =>
            {
                if (client.Request.HostKey == room.HostKey)
                    owners.AddOwner(clientLifetime, player);
            });
        }
    }
}
