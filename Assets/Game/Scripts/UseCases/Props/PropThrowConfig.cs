using UnityEngine;

namespace Bw.UseCases.Props
{
    [CreateAssetMenu(fileName = "PropThrowConfig", menuName = "Configs/PropThrowConfig")]
    public sealed class PropThrowConfig : ScriptableObject
    {
        [Min(0.01f)] public float Speed = 22f;
        [Min(0f)] public float Spin = 540f;

        [Header("Hit")]
        [Min(0f)] public float Damage = 5f;
        [Min(0f)] public float MinDamage = 1f;
        [Min(0f)] public float MaxDamage = 10f;
        [Min(0f)] public float Knockback = 8f;
        public LayerMask Targets = 1 << 15;
    }
}
