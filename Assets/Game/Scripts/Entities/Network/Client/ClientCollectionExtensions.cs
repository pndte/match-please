using System;

namespace Bw.Entities.Network
{
    public static class ClientCollectionExtensions
    {
        public static ulong IdOf(this IClientCollection clients, IClient client) =>
            clients.ByIds.TryGetLeft(client, out var id)
                ? id
                : throw new InvalidOperationException($"Client '{client}' has no id: only a connected client is in the collection.");
    }
}
