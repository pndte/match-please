using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public class NetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly DiContainer _container;
        private readonly GameObject _prefab;
        private readonly NetworkObject _prefabNetworkObject;

        public NetworkPrefabHandler(DiContainer container, GameObject prefab)
        {
            _container = container;
            _prefab = prefab;
            _prefabNetworkObject = prefab.GetComponent<NetworkObject>();
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            return NetworkPrefabInstantiationHelper.Instantiate(
                _container, _prefabNetworkObject, position, rotation);
        }

        public void Destroy(NetworkObject networkObject)
        {
            Object.Destroy(networkObject.gameObject);
        }
    }
}
