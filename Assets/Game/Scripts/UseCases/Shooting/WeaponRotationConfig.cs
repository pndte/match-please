using UnityEngine;

namespace Bw.UseCases.Shooting
{
    [CreateAssetMenu(fileName = "WeaponRotationConfig", menuName = "Configs/WeaponRotationConfig")]
    public class WeaponRotationConfig : ScriptableObject
    {
        [Header("Orbit Settings")]
        public float MinOrbitRadius = 1.0f;
        public float MaxOrbitRadius = 1.5f;
        public float RadiusChangeSpeed = 10f;

        [Header("Obstacle Detection")]
        public float ObstacleDetectionDistance = 1.5f;
        public LayerMask ObstacleLayerMask;

        [Header("Rotation Settings")]
        public float RotationSpeed = 0f;

        [Header("Weapon Orientation")]
        public bool RotateWeaponTowardsMouse = true;
    }
}

