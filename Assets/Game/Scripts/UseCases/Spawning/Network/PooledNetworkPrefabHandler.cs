using System.Collections.Generic;
using Bw.Entities.Pool;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class PooledNetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly Dictionary<NetworkObject, LifetimeDefinition> _lives = new();
        private readonly Lifetime _lifetime;
        private readonly IPool<NetworkObject> _pool;

        public PooledNetworkPrefabHandler(Lifetime lifetime, IPool<NetworkObject> pool)
        {
            _lifetime = lifetime;
            _pool = pool;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
        {
            var life = _lifetime.CreateNested();
            var networkObject = _pool.Resource(life.Lifetime);
            networkObject.transform.SetPositionAndRotation(position, rotation);
            _lives.AddLifetimed(life.Lifetime, new(networkObject, life));
            return networkObject;
        }

        public void Destroy(NetworkObject networkObject) =>
            _lives[networkObject].Terminate();
    }
}
