using System;
using Bw.Entities;
using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Clients.Extensions
{
    public static class ClientExtensions
    {
        public static void WhenConnected(this IClient client, Lifetime lifetime, Action<Lifetime> handler)
        {
            client.ConnectionState.View(lifetime, (connectedLifetime, state) =>
            {
                if (state == ClientConnectionState.Connected)
                    handler(connectedLifetime);
            });
        }
    }
}
