using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
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
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, ControlledByServicesInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public ControlledByServicesInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var meDeclaration = _netSchema.DeclareSignal<bool>(
                NetworkDelivery.Reliable,
                NetworkPermissions.Server);

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.BindNetSignalFor<bool, UseCases.ControlledBy.ClientNetworkHandler>(meDeclaration);
                Container.Bind<UseCases.ControlledBy.ClientNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindNetSignalFor<bool, UseCases.ControlledBy.ServerNetworkHandler>(meDeclaration);
                Container.Bind<UseCases.ControlledBy.ServerNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
        }
    }
}
