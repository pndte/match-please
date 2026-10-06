using System;

namespace Bw.Entities.Pool.GameObjects
{
    public static class PoolConfigExtensions
    {
        public static PoolSettings Settings(this PoolConfig config) =>
            new(config.Prewarm, config.Limit, config.CapMode switch
            {
                PoolCapMode.None => PoolCap.None,
                PoolCapMode.ReclaimOldest => PoolCap.ReclaimOldest(config.Cap),
                PoolCapMode.Throwing => PoolCap.Throwing(config.Cap),
                _ => throw new ArgumentOutOfRangeException(nameof(config.CapMode), config.CapMode, null),
            });
    }
}
