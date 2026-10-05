using UnityEngine;

namespace Bw.UseCases.Level.View.Platforms
{
    [CreateAssetMenu(fileName = "PlatformSkinConfig", menuName = "Configs/PlatformSkinConfig")]
    public sealed class PlatformSkinConfig : ScriptableObject
    {
        [Header("Drawing Beyond The Collider, Units")]
        public float OverhangLeft = 0.08f;
        public float OverhangBottom = 0.06f;
        public float OverhangRight = 0.08f;
        public float OverhangTop = 0.22f;
    }
}
