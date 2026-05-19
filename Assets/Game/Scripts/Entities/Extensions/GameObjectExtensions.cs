using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Extensions
{
    public static class GameObjectExtensions
    {
        public static Lifetime Lifetime(this GameObject gameObject)
        {
            if (gameObject.TryGetComponent<LifetimeHolder>(out var lifetimeHolder))
                return lifetimeHolder.GameObjectLifetime;
            
            var holder = gameObject.AddComponent<LifetimeHolder>();
            return holder.GameObjectLifetime;
        }
    }
}