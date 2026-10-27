using Bw.Entities;

namespace Bw.UseCases.Bots.Extensions
{
    public static class HealthExtensions
    {
        public static float Share(this IReadonlyHealth health) =>
            health.State.Value.Current / health.Max;
    }
}
