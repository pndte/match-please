using UnityEngine;

namespace Bw.UseCases.Props
{
    [CreateAssetMenu(fileName = "HeldPropConfig", menuName = "Configs/HeldPropConfig")]
    public sealed class HeldPropConfig : ScriptableObject
    {
        [Min(1f)] public float TurnSpeed = 480f;
        [Min(0f)] public float Gap = 0.05f;
        public LayerMask Layer = 1 << 17;
        public LayerMask Walls = 1 << 14;

        [Header("Placement")]
        [Min(0)] public int Pushes = 4;
        [Min(0f)] public float Stickiness = 0.3f;
        [Min(0f)] public float Slack = 0.3f;
        [Min(0f)] public float Nudge = 0.15f;

        [Header("Release")]
        [Min(1)] public int ReleaseRings = 3;
        [Range(1f, 180f)] public float ReleaseAngleStep = 5f;
    }
}
