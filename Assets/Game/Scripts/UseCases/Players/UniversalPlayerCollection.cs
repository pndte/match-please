using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using Bw.UseCases.Clients;
using Bw.UseCases.Clients.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Players
{
    public class UniversalPlayerCollection : IPlayerCollection, IClientPlayerCollection // TODO: maybe decompose
    {
        public IViewableBiMap<int, IPlayer> ById { get; }
        public IViewableBiMap<IClient, IPlayer> ByClient { get; }

        private int _indexCounter = 1;

        public UniversalPlayerCollection(Lifetime lifetime, IClientCollection collection)
        {
            ById = new ViewableBiMap<int, IPlayer>(lifetime);
            ByClient = new ViewableBiMap<IClient, IPlayer>(lifetime);
            collection.WhenNewConnected(lifetime, HandleNewConnectedClient);
        }

        private void HandleNewConnectedClient(Lifetime lifetime, IClient client)
        {
            var newPlayer = new Player();
            ById.AddLifetimed(lifetime, _indexCounter++, newPlayer);
            ByClient.AddLifetimed(lifetime, client, newPlayer);
        }
    }
}