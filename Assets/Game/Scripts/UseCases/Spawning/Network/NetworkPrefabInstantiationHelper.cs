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
            var inactiveParent = new GameObject(nameof(NetworkPrefabInstantiationHelper));
            inactiveParent.SetActive(false);
            inactiveParent.transform.SetParent(container.Resolve<Context>().transform, false);
            try
            {
                var instance = Object.Instantiate(prefab, position, rotation, inactiveParent.transform);
                container.InjectGameObject(instance.gameObject);
                instance.transform.SetParent(null, true);
                var context = instance.GetComponent<GameObjectContext>();
                if (context != null && context.Container == null)
                    context.Run();

                return instance;
            }
            finally
            {
                Object.Destroy(inactiveParent);
            }
        }
    }
}
