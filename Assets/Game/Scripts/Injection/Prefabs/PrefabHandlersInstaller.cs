using Bw.UseCases.Spawning.Network;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Prefabs
{
    public class PrefabHandlersInstaller : MonoInstaller
    {
        [SerializeField] private GameObject[] _networkPrefabs;

        public override void InstallBindings()
        {
            Container.BindInstance(_networkPrefabs).WhenInjectedInto<NetworkPrefabRegistrationBus>();
            Container.BindInterfacesTo<NetworkPrefabRegistrationBus>().AsSingle();
        }
    }
}
