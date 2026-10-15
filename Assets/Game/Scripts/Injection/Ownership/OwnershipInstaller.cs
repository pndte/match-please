using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.ObjectMeta.Network;
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
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind<IOwnership>().To<UseCases.Ownership>().AsSingle();
                Container.Bind<IOwnerRegistry>().To<UseCases.OwnerRegistry>().AsSingle();
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
                Container.BindResultReceiver(mineDeclaration).WhenInjectedInto<OwnershipClientHandler>();
                Container.Bind<OwnershipClientHandler>().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindResultSender(mineDeclaration).WhenInjectedInto<OwnershipServerHandler>();
                Container.Bind<OwnershipServerHandler>().AsSingle().NonLazy();
            }
        }
    }
}
