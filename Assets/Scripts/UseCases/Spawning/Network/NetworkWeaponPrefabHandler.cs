using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkWeaponPrefabHandler : INetworkPrefabInstanceHandler // TODO: refactor
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;
        private int _counter;

        public NetworkWeaponPrefabHandler(DiContainer container, GameObject prefab)
        {
            _container = container;
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var go = _container.InstantiatePrefab(_prefab, position, rotation, null);
            go.name += " " + _counter++;
            return go.GetComponent<NetworkObject>();
        }

        public void Destroy(NetworkObject networkObject)
        {
            if (networkObject != null)
                Object.Destroy(networkObject.gameObject);
        }
    }
}