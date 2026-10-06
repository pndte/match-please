using System;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    [Serializable]
    public sealed class PoolConfig
    {
        [Min(0)] public int Prewarm = 4;
        [Min(0)] public int Limit = 8;
        public PoolCapMode CapMode = PoolCapMode.ReclaimOldest;
        [Min(1)] public int Cap = 16;
    }
}
