using Bw.UseCases.Mortality;
using UnityEngine;

namespace Bw.UseCases.Props
{
    [CreateAssetMenu(fileName = "PropThrowConfig", menuName = "Configs/PropThrowConfig")]
    public sealed class PropThrowConfig : ScriptableObject, IHitConfig
    {
        [Min(0f)] public float Speed = 22f;
        [Min(0f)] public float Damage = 5f;
        [Min(0f)] public float Knockback = 8f;
        public LayerMask Targets = 1 << 15;

        float IHitConfig.Damage => Damage;
        float IHitConfig.Knockback => Knockback;
    }
}
