using Bw.Entities.Network;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class NetworkMatchSpawner
    {
        private readonly DiContainer _container;
        private readonly NetworkObject _prefab;

        public NetworkMatchSpawner(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            DiContainer container,
            NetworkObject prefab)
        {
            _container = container;
            _prefab = prefab;

            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ => Spawn());
        }

        private void Spawn() =>
            NetworkPrefabInstantiationHelper.Instantiate(_container, _prefab, Vector3.zero, Quaternion.identity)
                .Spawn(destroyWithScene: true);
    }
}
