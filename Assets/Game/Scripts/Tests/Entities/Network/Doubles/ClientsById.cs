using Bw.Entities.Infrastructure;
using Bw.Entities.Network;
using JetBrains.Lifetimes;

namespace Bw.Tests.Entities.Network.Doubles
{
    public sealed class ClientsById : IClientCollection
    {
        public IViewableBiMap<ulong, IClient> ByIds { get; }

        public ClientsById(Lifetime lifetime)
        {
            ByIds = new ViewableBiMap<ulong, IClient>(lifetime);
        }

        public Client Connect(ulong id, string nickname = "Саня")
        {
            var client = new Client(new ConnectionRequest(nickname, 1));
            ByIds.Add(id, client);
            return client;
        }
    }
}
