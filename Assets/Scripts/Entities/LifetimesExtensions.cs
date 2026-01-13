using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Entities
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
    }
}