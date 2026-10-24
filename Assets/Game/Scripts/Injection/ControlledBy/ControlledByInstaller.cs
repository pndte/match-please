using Bw.Entities;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network;
using Bw.UseCases.ObjectMeta.Network;
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
                Container.Bind(typeof(IReadonlyControlledBy), typeof(IControlledBy)).To<UseCases.ControlledBy>().AsSingle()
                    .WriterOnlyInto<IControlledBy>(typeof(ControlledByClientHandler));
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.Bind(typeof(IReadonlyControlledBy), typeof(IControlledBy)).To<UseCases.ControlledBy>().AsSingle()
                    .WriterOnlyInto<IControlledBy>(typeof(ControllerDrivers));
                Container.Bind<IControllerRegistry>().To<UseCases.ControllerRegistry>().AsSingle();
                Container.Bind<IControllingClients>().To<ControllingClients>().AsSingle();
                Container.Bind<ControllerDrivers>().AsSingle().NonLazy();
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
            var meDeclaration = _netSchema.DeclareResult<bool>(NetworkDelivery.Reliable);

            if (_runtimeSettings.CurrentPeerType == PeerType.Client)
            {
                Container.BindResultReceiver(meDeclaration).WhenInjectedInto<ControlledByClientHandler>();
                Container.Bind<ControlledByClientHandler>().AsSingle().NonLazy();
            }
            else if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                Container.BindResultSender(meDeclaration).WhenInjectedInto<ControlledByServerHandler>();
                Container.Bind<ControlledByServerHandler>().AsSingle().NonLazy();
            }
        }
    }
}
