using JetBrains.Collections.Viewable;
using Unity.Netcode;

namespace Bw.Entities.Network.Variables
{
    public interface INetPropertyFactory
    {
        public IViewableProperty<T> Viewable<T>(T initial, NetworkDelivery deliveryType);
        internal ISource<NetPropertyInfo> PropertyRegistered { get; }
    }
    
    public class NetPropertyFactory : INetPropertyFactory
    {
        ISource<NetPropertyInfo> INetPropertyFactory.PropertyRegistered => _propertyRegistered;
        private readonly Signal<NetPropertyInfo> _propertyRegistered = new();

        public IViewableProperty<T> Viewable<T>(T initial, NetworkDelivery deliveryType = NetworkDelivery.Reliable)
        {
            var property = new NetProperty<T>(initial);
            _propertyRegistered.Fire(new NetPropertyInfo(property, deliveryType));
            return property;
        }
    }

    public struct NetPropertyInfo
    {
        public INetProperty Property;
        public NetworkDelivery DeliveryType;

        public NetPropertyInfo(INetProperty property, NetworkDelivery deliveryType)
        {
            DeliveryType = deliveryType;
            Property = property;
        }
    }
}