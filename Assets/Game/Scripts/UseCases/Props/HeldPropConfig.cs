using UnityEngine;

namespace Bw.UseCases.Props
{
    [CreateAssetMenu(fileName = "HeldPropConfig", menuName = "Configs/HeldPropConfig")]
    public sealed class HeldPropConfig : ScriptableObject
    {
        [Min(1f)] public float TurnSpeed = 480f;
        [Min(0f)] public float Gap = 0.05f;
        public LayerMask Layer = 1 << 17;
    }
}
