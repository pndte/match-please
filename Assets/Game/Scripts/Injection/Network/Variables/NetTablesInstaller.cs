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
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind<INetEntries>().To<NetVariablesTableServer>().AsSingle().WithArguments(_schema).NonLazy();
                    break;
                case PeerType.Client:
                    Container.Bind<INetEntries>().To<NetVariablesTableClient>().AsSingle().WithArguments(_schema).NonLazy();
                    Container.Bind<NetSchemaHashSender>().AsSingle().WithArguments(_schema).NonLazy();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
