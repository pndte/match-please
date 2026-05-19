using JetBrains.Collections.Viewable;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetPropertyFactory
    {
        IViewableProperty<T> Viewable<T>(T initial, NetworkDelivery deliveryType, NetworkPermissions permissions);
        ISignal<T> Signal<T>(NetworkDelivery deliveryType, NetworkPermissions permissions);
        internal ISource<NetRegistryInfo> EntryRegistered { get; }
    }

    public class NetPropertyFactory : INetPropertyFactory
    {
        ISource<NetRegistryInfo> INetPropertyFactory.EntryRegistered => _entryRegistered;
        private readonly Signal<NetRegistryInfo> _entryRegistered = new();

        public IViewableProperty<T> Viewable<T>(T initial, NetworkDelivery deliveryType = NetworkDelivery.Reliable,
            NetworkPermissions permissions = NetworkPermissions.Server)
        {
            var property = new NetProperty<T>(initial);
            _entryRegistered.Fire(new NetRegistryInfo(property, deliveryType, permissions));
            return property;
        }

        public ISignal<T> Signal<T>(NetworkDelivery deliveryType = NetworkDelivery.Reliable,
            NetworkPermissions permissions = NetworkPermissions.Server)
        {
            var signal = new NetSignal<T>();
            _entryRegistered.Fire(new NetRegistryInfo(signal, deliveryType, permissions));
            return signal;
        }
    }

    public struct NetRegistryInfo
    {
        public INetSyncEntry Entry;
        public NetworkDelivery DeliveryType;
        public NetworkPermissions Permissions;

        public NetRegistryInfo(INetSyncEntry entry, NetworkDelivery deliveryType, NetworkPermissions permissions)
        {
            DeliveryType = deliveryType;
            Permissions = permissions;
            Entry = entry;
        }
    }
}
