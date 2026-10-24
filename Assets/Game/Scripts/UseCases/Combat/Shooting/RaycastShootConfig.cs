using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    [CreateAssetMenu(fileName = "RaycastShootConfig", menuName = "Configs/RaycastShootConfig")]
    public class RaycastShootConfig : ScriptableObject
    {
        [Header("Raycast")]
        public float MaxDistance = 20f;
        public LayerMask HitMask;
    }
}

