using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Spawning.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace Bw.Injection.Prefabs
{
    public class PrefabHandlersInstaller : MonoInstaller
    {
        [Inject] private SceneContext _sceneContext;
        [Inject] private IRuntimeSettings _runtimeSettings;
        [Inject] private PrefabPoolsConfig _pools;

        public override void InstallBindings()
        {
            BindPools();
            Container.Bind<NetworkPrefabHandlerFactory>().AsSingle().WithArguments(gameObject.Lifetime());
            Container.BindInterfacesTo<NetworkPrefabRegistrationBus>().AsSingle()
                .WithArguments(gameObject.Lifetime(), NetworkManager.Singleton);
        }

        private void BindPools()
        {
            var prefabs = PrefabPools.Create(gameObject.Lifetime(), _pools, _runtimeSettings.CurrentPeerType == PeerType.Client,
                (prefab, parent) => Instantiate(Container, prefab, parent));
            var pools = new MappedPools(new Dictionary<Type, IPool<object>>(), new Dictionary<Type, object> { [typeof(GameObject)] = prefabs });
            Container.Bind<IPools>().FromInstance(pools);
            _sceneContext.PostResolve += prefabs.Prewarm;
        }

        private static GameObject Instantiate(DiContainer container, GameObject prefab, Transform parent) =>
            prefab.TryGetComponent<NetworkObject>(out _)
                ? InstantiateNetwork(container, prefab)
                : container.InstantiatePrefab(prefab, parent);

        private static GameObject InstantiateNetwork(DiContainer container, GameObject prefab)
        {
            var active = prefab.activeSelf;
            prefab.SetActive(false);
            try
            {
                var instance = Object.Instantiate(prefab);
                Object.DontDestroyOnLoad(instance);
                container.InjectGameObject(instance);
                return instance;
            }
            finally
            {
                prefab.SetActive(active);
            }
        }
    }
}
