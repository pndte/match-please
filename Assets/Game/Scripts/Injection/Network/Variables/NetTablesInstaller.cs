using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Variables
{
    public class NetTablesInstaller : Installer<NetworkObject, NetTablesInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetPropertyFactory>().AsSingle();
            Container.Bind<INetVariablesTable>()
                .FromMethod(ResolveNetVariablesTable)
                .AsSingle();
        }

        private INetVariablesTable ResolveNetVariablesTable(InjectContext context)
        {
            var runtimeSettings = context.Container.Resolve<IRuntimeSettings>();
            return runtimeSettings.CurrentPeerType == PeerType.Server
                ? context.Container.Instantiate<NetVariablesTableServer>()
                : context.Container.Instantiate<NetVariablesTableClient>();
        }
    }
}
