using Bw.Entities.Network;
using Bw.Entities.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players.Network
{
    public sealed class ConnectedClientAdmitter
    {
        public ConnectedClientAdmitter(Lifetime lifetime, IClientCollection clients, IPlayerAdmitter admitter)
        {
            clients.ByIds.View(lifetime, (connectedLifetime, _, client) =>
                admitter.Admit(
                    connectedLifetime,
                    new Player(client.Request.Nickname),
                    client.Request.CharacterKey,
                    PlayerDriver.Client(client)));
        }
    }
}
