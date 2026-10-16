using UnityEngine;

namespace Bw.UseCases.Mortality
{
    [CreateAssetMenu(fileName = "HitImpulseConfig", menuName = "Configs/HitImpulseConfig")]
    public sealed class HitImpulseConfig : ScriptableObject
    {
        [Min(0.01f)] public float KnockbackMass = 1f;
        [Range(0f, 1f)] public float Spin = 0.25f;
    }
}
