using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkCharactersPrefabHandler : INetworkPrefabInstanceHandler // TODO: refactor
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;

        public NetworkCharactersPrefabHandler(DiContainer container, GameObject prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var go = UnityEngine.Object.Instantiate(_prefab, position, rotation);
            _container.InjectGameObject(go);
            return go.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            if (networkObject != null)
                Object.Destroy(networkObject.gameObject);
        }
    }
}