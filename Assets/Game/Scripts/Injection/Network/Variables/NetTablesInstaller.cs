using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Zenject;

namespace Bw.Injection.Network.Variables
{
    public class NetTablesInstaller : Installer<IRuntimeSettings, NetEntriesSchema, NetTablesInstaller>
    {
        private readonly IRuntimeSettings _runtimeSettings;
        private readonly NetEntriesSchema _schema;

        public NetTablesInstaller(IRuntimeSettings runtimeSettings, NetEntriesSchema schema)
        {
            _runtimeSettings = runtimeSettings;
            _schema = schema;
        }

        public override void InstallBindings()
        {
            Container.Bind(typeof(INetEntries), typeof(NetEntries)).To<NetEntries>().AsSingle().WithArguments(_schema);

            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<INetSendGuardFactory>().To<NetSendGuardFactoryServer>().AsSingle();
                    Container.Bind<NetReceiveGuardFactoryServer>().AsSingle();
                    Container.Bind<INetInbox>().To<NetInboxServer>().AsSingle();
                    Container.Bind<NetReplicationServer>().AsSingle().NonLazy();
                    Container.Bind<NetSchemaHashChecker>().AsSingle().WithArguments(_schema).NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<INetSendGuardFactory>().To<NetSendGuardFactoryClient>().AsSingle();
                    Container.Bind<INetInbox>().To<NetInboxClient>().AsSingle();
                    Container.Bind<NetReplicationClient>().AsSingle().NonLazy();
                    Container.Bind<NetSchemaHashSender>().AsSingle().WithArguments(_schema).NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
