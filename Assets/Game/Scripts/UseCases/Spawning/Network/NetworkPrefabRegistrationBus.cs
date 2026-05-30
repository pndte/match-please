using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkPrefabRegistrationBus : IInitializable
    {
        private readonly DiContainer _container;
        private readonly GameObject[] _prefabsToHandle;

        public NetworkPrefabRegistrationBus(DiContainer container, GameObject[] prefabsToHandle)
        {
            _container = container;
            _prefabsToHandle = prefabsToHandle;
        }

        public void Initialize()
        {
            foreach (var prefab in _prefabsToHandle)
            {
                var handler = new NetworkPrefabHandler(_container, prefab);
                NetworkManager.Singleton.PrefabHandler.AddHandler(prefab, handler);
            }
        }
    }
}
