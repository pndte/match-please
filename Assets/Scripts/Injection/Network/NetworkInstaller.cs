using Bw.Entities.Extensions;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    public class NetworkInstaller : MonoInstaller
    {
        [SerializeField] private NetworkObject _networkObject;
        
        public override void InstallBindings()
        {
            Container.BindInstance(gameObject.Lifetime());
            Container.Bind<NetworkManager>().FromInstance(NetworkManager.Singleton).AsSingle();
            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();
            Container.BindInterfacesTo<NetPropertyFactory>().AsSingle();
            MessageHandlersInstaller.Install(Container);
            
            Container.Bind<INetVariablesTable>().To<NetVariablesTable>().AsSingle();

            CreatePropertyFor<int, TestPropertyScript>(100);

            Container.Bind<TestPropertyScript>().AsSingle();
        }

        private void CreatePropertyFor<TValue, TDestination>(TValue initialValue, NetworkDelivery deliveryType = NetworkDelivery.Reliable)
        {
            Container.Bind<IViewableProperty<TValue>>()
                .FromMethod(ctx => {
                    var factory = ctx.Container.Resolve<INetPropertyFactory>();
                    return factory.Viewable(initialValue, deliveryType);
                })
                .WhenInjectedInto<TDestination>(); 
        }
    }
}