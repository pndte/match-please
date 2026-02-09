using UnityEngine;

namespace Bw.UseCases.Shooting
{
    [CreateAssetMenu(fileName = "LineRendererVfxConfig", menuName = "Configs/LineRendererVfxConfig")]
    public class LineRendererVfxConfig : ScriptableObject
    {
        [Header("Trail VFX")]
        public float TrailSpeed = 10f;
        public float TrailLength = 1f;
        public Color TrailColor = Color.white;
    }
}