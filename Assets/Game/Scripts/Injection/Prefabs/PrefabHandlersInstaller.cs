using Bw.Entities.Extensions;
using Bw.UseCases.Spawning.Network;
using Unity.Netcode;
using Zenject;

namespace Bw.Injection.Prefabs
{
    public class PrefabHandlersInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<NetworkPrefabRegistrationBus>().AsSingle()
                .WithArguments(gameObject.Lifetime(), NetworkManager.Singleton);
        }
    }
}
