using JetBrains.Collections.Viewable;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface INetworkHolder //TODO: мб как-то можно избавиться от этого
    {
        public IViewableProperty<NetworkManager> NetworkManager { get; }
    }

    public class NetworkHolder : INetworkHolder
    {
        public IViewableProperty<NetworkManager> NetworkManager { get; } = new ViewableProperty<NetworkManager>();
    }
}