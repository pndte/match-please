using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Ownership
{
    public class OwnershipInstaller 
        : Installer<IRuntimeSettings, OwnershipInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public OwnershipInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IReadonlyOwnership>().To<UseCases.Ownership>().AsSingle();
                Container.Bind<UseCases.Ownership>()
                    .FromMethod(ctx => (UseCases.Ownership)ctx.Container.Resolve<IReadonlyOwnership>())
                    .WhenInjectedInto<UseCases.Ownership.ClientNetworkHandler>();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<UseCases.Ownership>().AsSingle();
            }
        }
    }

    public class OwnershipServicesInstaller
        : Installer<IRuntimeSettings, INetPropertyFactory, OwnershipServicesInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetPropertyFactory _netPropertyFactory;

        public OwnershipServicesInstaller(
            IRuntimeSettings runtimeSettings,
            INetPropertyFactory netPropertyFactory)
        {
            _runtimeSettings = runtimeSettings;
            _netPropertyFactory = netPropertyFactory;
        }

        public override void InstallBindings()
        {
            var handler =
                new DtoHandler<OwnershipDto>(_netPropertyFactory.Signal<OwnershipDto>(NetworkDelivery.Reliable, NetworkPermissions.Server));
            
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IDtoSource<OwnershipDto>>().To<DtoHandler<OwnershipDto>>().FromInstance(handler).AsSingle();
                Container.Bind<UseCases.Ownership.ClientNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind<IDtoBroadcaster<OwnershipDto>>().To<DtoHandler<OwnershipDto>>().FromInstance(handler).AsSingle();
                Container.Bind<UseCases.Ownership.ServerNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
                
        }
    }
}