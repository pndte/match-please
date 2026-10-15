using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class OwnershipServerHandler
    {
        public OwnershipServerHandler(
            Lifetime lifetime,
            INetResultSender<bool> mine,
            IOwnerRegistry ownerRegistry,
            IClientPlayerCollection clientPlayers)
        {
            ownerRegistry.Owners.View(lifetime, (ownerLifetime, owner) =>
            {
                var client = clientPlayers.ByClient.Inverse[owner]; //TODO: у бота клиента нет — когда появятся боты, игроков без клиента здесь пропускать
                mine.SendTo(client, true);

                ownerLifetime.OnTermination(() =>
                    mine.SendTo(client, false));
            });
        }
    }
}
