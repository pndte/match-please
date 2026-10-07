using System;
using JetBrains.Lifetimes;

namespace Bw.Entities.Extensions
{
    public static class HealthExtensions
    {
        public static void AdviseDamage(this IReadonlyHealth health, Lifetime lifetime, Action<float> handler)
        {
            health.Changed.Advise(lifetime, change =>
            {
                if (change < 0f)
                    handler(-change);
            });
        }
    }
}
