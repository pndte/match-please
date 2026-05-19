using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Infrastructure;
using Bw.UseCases.Clients;
using JetBrains.Collections.Viewable;
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
            collection.ByIds.View(lifetime, HandleNewClient);
        }

        private void HandleNewClient(Lifetime lifetime, ulong _, IClient client)
        {
            var newPlayer = new Player();
            ById.AddLifetimed(lifetime, _indexCounter++, newPlayer);
            ByClient.AddLifetimed(lifetime, client, newPlayer);
        }
    }
}