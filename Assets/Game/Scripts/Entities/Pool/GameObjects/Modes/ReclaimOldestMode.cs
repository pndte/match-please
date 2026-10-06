using System;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects.Modes
{
    [Serializable]
    public sealed class ReclaimOldestMode : IPoolCapMode
    {
        [Min(1)] public int Max = 16;

        public PoolCap Cap() =>
            PoolCap.ReclaimOldest(Max);
    }
}
