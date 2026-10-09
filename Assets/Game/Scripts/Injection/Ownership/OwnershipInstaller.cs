using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Ownership
{
    public class OwnershipInstaller : Installer<IRuntimeSettings, OwnershipInstaller>
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
                Container.Bind<IOwnership>().To<UseCases.Ownership>().AsSingle();
                Container.Bind<UseCases.Ownership>()
                    .FromMethod(ctx => (UseCases.Ownership)ctx.Container.Resolve<IOwnership>())
                    .WhenInjectedInto<UseCases.Ownership.ClientNetworkHandler>();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindInterfacesTo<UseCases.Ownership>().AsSingle();
            }
        }
    }

    public class OwnershipServicesInstaller
        : Installer<IRuntimeSettings, INetEntriesSchemaBuilder, OwnershipServicesInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly INetEntriesSchemaBuilder _netSchema;

        public OwnershipServicesInstaller(
            IRuntimeSettings runtimeSettings,
            INetEntriesSchemaBuilder netSchema)
        {
            _runtimeSettings = runtimeSettings;
            _netSchema = netSchema;
        }

        public override void InstallBindings()
        {
            var mineDeclaration = _netSchema.DeclareResult<bool>(NetworkDelivery.Reliable);

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.BindResultReceiver(mineDeclaration).WhenInjectedInto<UseCases.Ownership.ClientNetworkHandler>();
                Container.Bind<UseCases.Ownership.ClientNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindResultSender(mineDeclaration).WhenInjectedInto<UseCases.Ownership.ServerNetworkHandler>();
                Container.Bind<UseCases.Ownership.ServerNetworkHandler>().ToSelf().AsSingle().NonLazy();
            }
        }
    }
}
