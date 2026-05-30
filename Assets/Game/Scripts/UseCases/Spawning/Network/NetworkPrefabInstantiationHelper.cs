using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Spawning.Network
{
    internal static class NetworkPrefabInstantiationHelper //TODO: wtf
    {
        public static NetworkObject Instantiate(
            DiContainer container,
            NetworkObject prefab,
            Vector3 position,
            Quaternion rotation)
        {
            var instance = container.InstantiatePrefabForComponent<NetworkObject>(
                prefab, position, rotation, null);
            var context = instance.GetComponent<GameObjectContext>();
            if (context != null && context.Container == null)
                context.Run();
            return instance;
        }
    }
}
