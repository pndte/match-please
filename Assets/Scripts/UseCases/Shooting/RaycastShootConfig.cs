using UnityEngine;

namespace Bw.UseCases.Shooting
{
    [CreateAssetMenu(fileName = "RaycastShootConfig", menuName = "Configs/RaycastShootConfig")]
    public class RaycastShootConfig : ScriptableObject
    {
        [Header("Raycast")]
        public float MaxDistance = 20f;
        public float Damage = 10f;
        public LayerMask HitMask;

        [Header("Trail VFX")]
        public float TrailDuration = 0.15f;
        public float TrailSpeed = 10f;
        public float TrailLength = 1f;
        public Color TrailColor = Color.white;
    }
}

