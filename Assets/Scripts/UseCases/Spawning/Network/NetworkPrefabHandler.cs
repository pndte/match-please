using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly IInstantiator _instantiator;
        private readonly GameObject _prefab;

        public NetworkPrefabHandler(IInstantiator instantiator, GameObject prefab)
        {
            _instantiator = instantiator;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var go = _instantiator.InstantiatePrefab(_prefab, position, rotation, null);
            return go.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}