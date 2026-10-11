using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    [CreateAssetMenu(fileName = "StrikeFeelConfig", menuName = "Configs/StrikeFeelConfig")]
    public sealed class StrikeFeelConfig : ScriptableObject
    {
        [Min(0f)] public float HitStop = 0.06f;
        [Min(0f)] public float CameraKick = 0.22f;
    }
}
