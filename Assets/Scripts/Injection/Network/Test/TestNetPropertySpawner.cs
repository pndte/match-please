using Bw.Entities.Network;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    public class TestNetPropertySpawner : MonoBehaviour
    {
        [Inject] IRuntimeSettings _runtimeSettings;
        [Inject] IInstantiator _instantiator;
        [SerializeField] private NetworkObject _networkObject;

        private void Awake()
        {
            if (_runtimeSettings.CurrentPeerType == PeerType.Server)
            {
                var prefab = _instantiator.InstantiatePrefabForComponent<NetworkObject>(_networkObject);
                prefab.Spawn();
            }
        }
    }
}