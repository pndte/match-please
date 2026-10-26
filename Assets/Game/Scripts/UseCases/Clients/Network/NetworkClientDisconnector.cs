using Bw.Entities.Network;

namespace Bw.UseCases.Clients.Network
{
    public sealed class NetworkClientDisconnector : IClientDisconnector
    {
        private readonly INetworkHolder _network;

        public NetworkClientDisconnector(INetworkHolder network)
        {
            _network = network;
        }

        public void Disconnect(ulong clientId, string reason) =>
            _network.NetworkManager.Value.DisconnectClient(clientId, reason);
    }
}
