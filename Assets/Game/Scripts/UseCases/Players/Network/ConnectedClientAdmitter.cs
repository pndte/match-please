using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Clients.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players.Network
{
    public sealed class ConnectedClientAdmitter
    {
        public ConnectedClientAdmitter(Lifetime lifetime, IClientCollection clients, IPlayerAdmitter admitter)
        {
            clients.WhenNewConnected(lifetime, (connectedLifetime, client) =>
                admitter.Admit(
                    connectedLifetime,
                    new Player(client.Request.Nickname),
                    client.Request.CharacterKey,
                    PlayerDriver.Client(client)));
        }
    }
}
