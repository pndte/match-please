using System;
using JetBrains.Lifetimes;

namespace Bw.Entities.Extensions
{
    public static class HealthExtensions
    {
        public static void AdviseDamage(this IReadonlyHealth health, Lifetime lifetime, Action<float> handler)
        {
            var previous = health.Current.Value;
            health.Current.Advise(lifetime, current =>
            {
                if (current < previous)
                    handler(previous - current);
                previous = current;
            });
        }
    }
}
