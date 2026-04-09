using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network
{
    public static class NetworkInjectionExtensions
    {
        public static void CreatePropertyFor<TValue, TDestination>(this DiContainer container, TValue initialValue = default,
            NetworkDelivery deliveryType = NetworkDelivery.Reliable, NetworkPermissions permissions = NetworkPermissions.Server)
        {
            container.Bind<IViewableProperty<TValue>>()
                .FromMethod(ctx => {
                    var factory = ctx.Container.Resolve<INetPropertyFactory>();
                    return factory.Viewable(initialValue, deliveryType, permissions);
                })
                .WhenInjectedInto<TDestination>(); 
        }
    }
}