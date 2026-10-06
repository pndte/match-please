using UnityEngine;

namespace Bw.UseCases.Camera.View.Follow
{
    [CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "Configs/CameraFollowConfig")]
    public class CameraFollowConfig : ScriptableObject
    {
        [Header("Framing")]
        public Vector2 Offset = new(0f, 1.5f);
        [Min(0.01f)] public float HorizontalSmoothing = 0.18f;
        [Min(0.01f)] public float VerticalSmoothing = 0.3f;

        [Header("Cursor")]
        public Vector2 MaxLookAhead = new(4f, 2.5f);
        [Min(1f)] public float LookAheadCurve = 1.5f;
        [Min(0.01f)] public float LookAheadSmoothing = 0.35f;
    }
}
