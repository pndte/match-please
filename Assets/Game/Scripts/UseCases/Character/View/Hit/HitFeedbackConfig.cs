using Bw.UseCases.Mortality.View.Hit;
using UnityEngine;

namespace Bw.UseCases.Character.View.Hit
{
    [CreateAssetMenu(fileName = "HitFeedbackConfig", menuName = "Configs/HitFeedbackConfig")]
    public class HitFeedbackConfig : ScriptableObject, IHitFlashConfig
    {
        [Header("Flash")]
        public Color FlashColor = new(1f, 0.38f, 0.32f, 1f);
        [Range(0f, 1f)] public float FlashStrength = 0.85f;
        [Min(0.01f)] public float FlashDuration = 0.18f;

        [Header("Camera")]
        [Range(0f, 1f)] public float CameraShake = 0.55f;

        Color IHitFlashConfig.FlashColor => FlashColor;
        float IHitFlashConfig.FlashStrength => FlashStrength;
        float IHitFlashConfig.FlashDuration => FlashDuration;
    }
}
