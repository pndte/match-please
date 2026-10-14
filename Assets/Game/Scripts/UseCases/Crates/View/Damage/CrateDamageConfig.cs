using UnityEngine;

namespace Bw.UseCases.Crates.View.Damage
{
    [CreateAssetMenu(fileName = "CrateDamageConfig", menuName = "Configs/CrateDamageConfig")]
    public sealed class CrateDamageConfig : ScriptableObject
    {
        public Sprite Cracked;
        [Range(0f, 1f)] public float CrackedBelow = 0.5f;

        [Header("Hit")]
        public Vector3 HitPunch = new(0.1f, -0.1f, 0f);
        [Min(0.01f)] public float HitPunchTime = 0.18f;
    }
}
