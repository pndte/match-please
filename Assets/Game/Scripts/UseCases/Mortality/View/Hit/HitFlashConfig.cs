using UnityEngine;

namespace Bw.UseCases.Mortality.View.Hit
{
    [CreateAssetMenu(fileName = "HitFlashConfig", menuName = "Configs/HitFlashConfig")]
    public class HitFlashConfig : ScriptableObject, IHitFlashConfig
    {
        public Color FlashColor = Color.white;
        [Range(0f, 1f)] public float FlashStrength = 0.85f;
        [Min(0.01f)] public float FlashDuration = 0.18f;

        Color IHitFlashConfig.FlashColor => FlashColor;
        float IHitFlashConfig.FlashStrength => FlashStrength;
        float IHitFlashConfig.FlashDuration => FlashDuration;
    }
}
