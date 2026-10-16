using UnityEngine;

namespace Bw.UseCases.Vfx.View.Collisions
{
    [CreateAssetMenu(fileName = "ParticleCollisionConfig", menuName = "Configs/ParticleCollisionConfig")]
    public sealed class ParticleCollisionConfig : ScriptableObject
    {
        public LayerMask Surfaces = 1 << 14;
        public ParticleSystem.MinMaxCurve Bounce = 0f;
        public ParticleSystem.MinMaxCurve Dampen = 0f;
        public ParticleSystem.MinMaxCurve LifetimeLoss = 0f;
        [Min(0f)] public float RadiusScale = 1f;
    }
}
