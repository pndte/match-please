using System;
using Bw.Entities.Pool.GameObjects;

namespace Bw.UseCases.Shooting.View.Impact
{
    [Serializable]
    public sealed class ShotImpactConfig
    {
        public ParticleImpactEffect Prefab;
        public PoolConfig Pool = new();
    }
}
