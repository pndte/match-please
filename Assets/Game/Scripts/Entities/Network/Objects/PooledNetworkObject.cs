using Bw.Entities.Pool;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Objects
{
    public sealed class PooledNetworkObject : MonoBehaviour, IResource<PooledNetworkObject>
    {
        [SerializeField] private NetworkObject _networkObject;

        public PooledNetworkObject Facade(Lifetime lifetime)
        {
            lifetime.OnTermination(Despawn);
            return this;
        }

        public void Spawn(Vector3 position, Quaternion rotation)
        {
            transform.SetPositionAndRotation(position, rotation);
            _networkObject.Spawn(destroyWithScene: true);
        }

        private void Despawn()
        {
            if (_networkObject.IsSpawned)
                _networkObject.Despawn();
        }
    }
}
