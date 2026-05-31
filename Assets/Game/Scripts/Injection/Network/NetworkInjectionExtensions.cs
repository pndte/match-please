using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public static class NetworkInjectionExtensions
    {
        public static void CreatePropertyFor<TValue, TDestination>(this DiContainer container,
            TValue initialValue = default,
            NetworkDelivery deliveryType = NetworkDelivery.Reliable,
            NetworkPermissions permissions = NetworkPermissions.Server)
        {
            var factory = container.Resolve<INetPropertyFactory>();
            var property = factory.Viewable(initialValue, deliveryType, permissions);

            container.Bind<IViewableProperty<TValue>>()
                .FromInstance(property)
                .WhenInjectedInto<TDestination>();
        }

        public static void CreateSignalFor<TValue, TDestination>(this DiContainer container,
            NetworkDelivery deliveryType = NetworkDelivery.Reliable,
            NetworkPermissions permissions = NetworkPermissions.Server)
        {
            var factory = container.Resolve<INetPropertyFactory>();
            var signal = factory.Signal<TValue>(deliveryType, permissions);
            
            container.Bind<ISignal<TValue>>()
                .FromInstance(signal)
                .WhenInjectedInto<TDestination>();
        }
    }
}