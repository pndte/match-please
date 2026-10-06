using System;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    [Serializable]
    public sealed class PrefabPoolsConfig
    {
        [Min(0)] public int UnlistedLimit = 8;
        public PrefabPoolConfig[] Prefabs = Array.Empty<PrefabPoolConfig>();
    }
}
