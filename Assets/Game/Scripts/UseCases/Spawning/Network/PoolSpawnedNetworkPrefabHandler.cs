using System;
using Bw.Entities.Network.Objects;
using Unity.Netcode;
using UnityEngine;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class PoolSpawnedNetworkPrefabHandler : INetworkPrefabInstanceHandler
    {
        private readonly GameObject _prefab;

        public PoolSpawnedNetworkPrefabHandler(GameObject prefab)
        {
            _prefab = prefab;
        }

        public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation) =>
            throw new InvalidOperationException(
                $"{_prefab.name} is pooled: take it from the pool and spawn it through {nameof(PooledNetworkObject)}, NGO doesn't instantiate it.");

        public void Destroy(NetworkObject networkObject)
        {
            //TODO: деспавн, начатый не концом выдачи (выключение сети, прямой Despawn()), объект в пул не возвращает: он остаётся выданным и включённым, пока не кончится выдача
        }
    }
}
