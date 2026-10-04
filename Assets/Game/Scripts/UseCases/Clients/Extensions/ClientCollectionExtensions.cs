using System;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Clients.Extensions
{
    public static class ClientCollectionExtensions
    {
        public static void WhenNewConnected(
            this IClientCollection collection,
            Lifetime lifetime,
            Action<Lifetime, IClient> handler)
        {
            collection.ByIds.View(lifetime, (clientLifetime, _, client) =>
            {
                client.WhenConnected(clientLifetime, connectedLifetime => handler(connectedLifetime, client));
            });
        }
    }
}
