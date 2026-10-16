using System;
using Bw.UseCases.Vfx.View.Collisions;
using UnityEngine;

namespace Bw.Injection.Vfx
{
    [Serializable]
    public struct CollidingParticles
    {
        public ParticleSystem Particles;
        public ParticleCollisionConfig Config;
    }
}
