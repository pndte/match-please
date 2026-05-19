using Unity.Netcode;

namespace Bw.Entities.Network
{
    public static class NetworkHolderExtensions
    {
        public static NetworkSpawnManager SpawnManager(this INetworkHolder networkHolder)
        {
            return networkHolder.NetworkManager.Value.SpawnManager;
        }
        
        public static CustomMessagingManager CustomMessagingManager(this INetworkHolder networkHolder)
        {
            return networkHolder.NetworkManager.Value.CustomMessagingManager;
        }
    }
}