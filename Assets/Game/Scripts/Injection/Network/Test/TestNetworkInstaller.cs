using Bw.Entities.Extensions;
using Bw.Injection.Network.Variables;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    public class TestNetworkInstaller : MonoInstaller
    {
        [SerializeField] private NetworkObject _networkObject;

        public override void InstallBindings()
        {
            Container.Bind<NetworkManager>().FromInstance(NetworkManager.Singleton).AsSingle();
            MessageHandlersInstaller.Install(Container);
            Container.BindInstance(gameObject.Lifetime());
            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();

            NetTablesInstaller.Install(Container);

            Container.CreatePropertyFor<int, TestPropertyScript>(100);

            Container.Bind<TestPropertyScript>().AsSingle();
        }
    }
}
