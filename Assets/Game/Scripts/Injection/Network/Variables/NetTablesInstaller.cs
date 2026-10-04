using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Zenject;

namespace Bw.Injection.Network.Variables
{
    public class NetTablesInstaller : Installer<NetEntriesSchema, NetTablesInstaller>
    {
        private readonly NetEntriesSchema _schema;

        public NetTablesInstaller(NetEntriesSchema schema)
        {
            _schema = schema;
        }

        public override void InstallBindings()
        {
            Container.Bind<INetEntries>()
                .FromMethod(CreateNetVariablesTable)
                .AsSingle()
                .NonLazy();
        }

        private NetVariablesTableBase CreateNetVariablesTable(InjectContext context)
        {
            var runtimeSettings = context.Container.Resolve<IRuntimeSettings>();
            var arguments = new object[] { _schema };
            return runtimeSettings.CurrentPeerType == PeerType.Server
                ? context.Container.Instantiate<NetVariablesTableServer>(arguments)
                : context.Container.Instantiate<NetVariablesTableClient>(arguments);
        }
    }
}
