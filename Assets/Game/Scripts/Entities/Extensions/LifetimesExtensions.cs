using System;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Extensions
{
    public static class LifetimesExtensions
    {
        public static void WhenAlive(this IReadonlyProperty<Lifetime> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            property.Advise(lifetime, value =>
            {
                if (value.IsAlive)
                    handler(value);
            });
        }
        
        public static void WhenAliveOnce(this IReadonlyProperty<Lifetime> property, Lifetime lifetime, Action<Lifetime> handler)
        {
            var def = lifetime.CreateNested();
            property.Advise(def.Lifetime, value =>
            {
                if (value.IsAlive)
                {
                    handler(value);
                    def.Terminate();
                }
            });
        }
    }
}