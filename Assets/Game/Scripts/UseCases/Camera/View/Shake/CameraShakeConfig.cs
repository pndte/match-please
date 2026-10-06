using UnityEngine;

namespace Bw.UseCases.Camera.View.Shake
{
    [CreateAssetMenu(fileName = "CameraShakeConfig", menuName = "Configs/CameraShakeConfig")]
    public class CameraShakeConfig : ScriptableObject
    {
        [Header("Shake")]
        [Min(0f)] public float MaxOffset = 0.35f;
        [Min(0f)] public float MaxAngle = 1.2f;
        [Min(0f)] public float Frequency = 20f;
        [Min(0f)] public float Decay = 1.8f;

        [Header("Kick")]
        [Min(0f)] public float MaxKick = 0.25f;
        [Min(0f)] public float KickReturn = 14f;
        [Min(0f)] public float MaskingPanSpeed = 2f;
    }
}
