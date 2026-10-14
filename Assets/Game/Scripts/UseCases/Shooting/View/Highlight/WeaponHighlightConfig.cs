using UnityEngine;

namespace Bw.UseCases.Shooting.View.Highlight
{
    [CreateAssetMenu(fileName = "WeaponHighlightConfig", menuName = "Configs/WeaponHighlightConfig")]
    public sealed class WeaponHighlightConfig : ScriptableObject
    {
        public Material Material;
        [Min(0.01f)] public float FadeInTime = 0.12f;
        [Min(0.01f)] public float FadeOutTime = 0.2f;
    }
}
