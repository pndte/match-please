using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.Entities.Extensions.Zenject
{
    public static class ZenjectExtensions
    {
        public static T InstantiatePrefabForComponent<T>(this IInstantiator container, Lifetime lifetime, NetworkObject prefab,
            Vector3 position, Quaternion rotation, Transform parent = null) where T: Component
        {
            var go = container.InstantiatePrefabForComponent<T>(prefab, position, rotation, parent);
            lifetime.OnTermination(() =>
                prefab.Despawn(true));
            
            return go.GetComponent<T>();
        }
    }
}