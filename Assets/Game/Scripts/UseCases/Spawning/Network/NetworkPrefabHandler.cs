using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;

        public NetworkPrefabHandler(DiContainer container, GameObject prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var go = Object.Instantiate(_prefab, position, rotation);
            _container.InjectGameObject(go);
            return go.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}