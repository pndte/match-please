using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Clients.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players.Network
{
    public sealed class ClientPlayerAdmission
    {
        public ClientPlayerAdmission(Lifetime lifetime, IClientCollection clients, IPlayerEntrance entrance)
        {
            clients.WhenNewConnected(lifetime, (connectedLifetime, client) =>
                entrance.Admit(
                    connectedLifetime,
                    new Player(client.Request.Nickname),
                    client.Request.CharacterKey,
                    PlayerDriver.Client(client)));
        }
    }
}
