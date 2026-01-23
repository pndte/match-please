using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Extensions
{
    public static class LifetimesExtensions
    {
        public static void WhenAlive(this IReadonlyProperty<Lifetime> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            property.View(lifetime, (valueLifetime, value) =>
            {
                if (value.IsAlive)
                {
                    handler(valueLifetime);
                }
            });
        }
        
        public static void WhenAliveOnce(this IReadonlyProperty<Lifetime> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            var def = lifetime.CreateNested();
            property.View(def.Lifetime, (valueLifetime, value) =>
            {
                if (value.IsAlive)
                {
                    handler(valueLifetime);
                    def.Terminate();
                }
            });
        }
    }
}