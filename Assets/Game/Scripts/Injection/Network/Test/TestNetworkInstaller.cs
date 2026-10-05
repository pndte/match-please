using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Injection.Network.Variables;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    public class TestNetworkInstaller : MonoInstaller //TODO: нигде не используется и в рантайме не соберётся: таблице не хватает владения и коллекций клиентов, отправителю хеша схемы — INetworkLifetimedObject
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        [SerializeField] private NetworkObject _networkObject;

        public override void InstallBindings()
        {
            Container.Bind<NetworkManager>().FromInstance(NetworkManager.Singleton).AsSingle();
            MessageHandlersInstaller.Install(Container);
            Container.BindInstance(gameObject.Lifetime());
            Container.Bind<NetworkObject>().FromInstance(_networkObject).AsSingle();

            var netSchema = new NetEntriesSchemaBuilder();
            var testPropertyDeclaration = netSchema.DeclareProperty(100, NetworkDelivery.Reliable, NetworkPermissions.Server);
            Container.BindNetPropertyFor<int, TestPropertyScript>(testPropertyDeclaration);
            NetTablesInstaller.Install(Container, _runtimeSettings, netSchema.Build());

            Container.Bind<TestPropertyScript>().AsSingle();
        }
    }
}
