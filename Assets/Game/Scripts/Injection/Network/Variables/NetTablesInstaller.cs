using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Zenject;

namespace Bw.Injection.Network.Variables
{
    public class NetTablesInstaller : Installer<NetTablesInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetPropertyFactory>().AsSingle();
            Container.Bind<INetVariablesTable>()
                .FromMethod(ResolveNetVariablesTable)
                .AsSingle()
                .NonLazy();
        }

        private static INetVariablesTable ResolveNetVariablesTable(InjectContext context)
        {
            var runtimeSettings = context.Container.Resolve<IRuntimeSettings>();
            return runtimeSettings.CurrentPeerType == PeerType.Server
                ? context.Container.Instantiate<NetVariablesTableServer>()
                : context.Container.Instantiate<NetVariablesTableClient>();
        }
    }
}
