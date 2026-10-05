using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Clients.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players.Network
{
    public class NetworkPlayerCollection : IClientPlayerCollection
    {
        public IViewableBiMap<IClient, IPlayer> ByClient { get; }

        private readonly IPlayerCollection _players;

        public NetworkPlayerCollection(Lifetime lifetime, IClientCollection clients, IPlayerCollection players)
        {
            _players = players;
            ByClient = new ViewableBiMap<IClient, IPlayer>(lifetime);
            clients.WhenNewConnected(lifetime, HandleNewConnectedClient);
        }

        private void HandleNewConnectedClient(Lifetime lifetime, IClient client)
        {
            var player = new Player();
            ByClient.AddLifetimed(lifetime, client, player);
            _players.AddLifetimed(lifetime, player);
        }
    }
}