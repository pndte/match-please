using Bw.Entities.Network.Variables;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Network.Variables
{
    public class NetTablesInstaller : Installer<NetworkObject, NetTablesInstaller>
    {
        [Inject] NetworkObject _networkObject;
        
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetPropertyFactory>().AsSingle();
            Container.Bind<INetVariablesTable>().To<NetVariablesTable>().AsSingle().WithArguments(_networkObject);
        }
    }
}