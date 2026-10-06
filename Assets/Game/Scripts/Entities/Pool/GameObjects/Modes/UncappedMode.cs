using System;

namespace Bw.Entities.Pool.GameObjects.Modes
{
    [Serializable]
    public sealed class UncappedMode : IPoolCapMode
    {
        public PoolCap Cap() =>
            PoolCap.None;
    }
}
