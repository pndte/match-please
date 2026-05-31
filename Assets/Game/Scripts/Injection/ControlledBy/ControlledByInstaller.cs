using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.ControlledBy
{
    public class ControlledByInstaller : Installer<IRuntimeSettings, ControlledByInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public ControlledByInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IReadonlyControlledBy>().To<UseCases.ControlledBy>().AsSingle();
                Container.Bind<UseCases.ControlledBy>()
                    .FromMethod(ctx => (UseCases.ControlledBy)ctx.Container.Resolve<IReadonlyControlledBy>())
                    .WhenInjectedInto<UseCases.ControlledBy.ClientNetworkHandler>();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<UseCases.ControlledBy>().AsSingle();
            }
        }
    }

    public class ControlledByServicesInstaller
        : Installer<IRuntimeSettings, INetPropertyFactory, ControlledByServicesInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetPropertyFactory _netPropertyFactory;

        public ControlledByServicesInstaller(
            IRuntimeSettings runtimeSettings,
            INetPropertyFactory netPropertyFactory)
        {
            _runtimeSettings = runtimeSettings;
            _netPropertyFactory = netPropertyFactory;
        }

        public override void InstallBindings()
        {
            var handler = new DtoHandler<TargetedBool>(
                _netPropertyFactory.Signal<TargetedBool>(
                    NetworkDelivery.Reliable,
                    NetworkPermissions.Server));

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.Bind<IDtoSource<TargetedBool>>()
                    .To<DtoHandler<TargetedBool>>()
                    .FromInstance(handler)
                    .WhenInjectedInto<UseCases.ControlledBy.ClientNetworkHandler>();
                Container.Bind<UseCases.ControlledBy.ClientNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind<IDtoBroadcaster<TargetedBool>>()
                    .To<DtoHandler<TargetedBool>>()
                    .FromInstance(handler)
                    .WhenInjectedInto<UseCases.ControlledBy.ServerNetworkHandler>();
                Container.Bind<UseCases.ControlledBy.ServerNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
        }
    }
}
