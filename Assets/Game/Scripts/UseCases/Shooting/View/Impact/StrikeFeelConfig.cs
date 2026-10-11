using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    [CreateAssetMenu(fileName = "StrikeFeelConfig", menuName = "Configs/StrikeFeelConfig")]
    public sealed class StrikeFeelConfig : ScriptableObject
    {
        [Min(0f)] public float HitStop = 0.06f;
        [Min(0f)] public float CameraKick = 0.22f;

        [Header("Full charge")]
        public Color FullChargeTint = new(1f, 0.42f, 0.22f, 1f);
        [Min(0.01f)] public float FullChargeFlashTime = 0.07f;
        [Range(0f, 1f)] public float FullChargePulseLow = 0.4f;
        [Min(0.01f)] public float FullChargePulseTime = 0.18f;
    }
}
