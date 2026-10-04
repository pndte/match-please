using UnityEngine;

namespace DefaultNamespace
{
    /// <summary>
    /// Configuration for camera follow behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "CameraFollowConfig", menuName = "Configs/CameraFollowConfig")]
    public class CameraFollowConfig : ScriptableObject
    {
        [Header("Follow Settings")]
        [Tooltip("How fast the camera catches up with the target, per second (higher = tighter). Independent of frame rate.")]
        [Min(0f)]
        public float FollowSharpness = 20f;
        
        [Header("Offset Settings")]
        [Tooltip("Offset from the target position")]
        public Vector3 Offset = new Vector3(0f, 0f, -10f);
        
        [Header("Boundaries (Optional)")]
        [Tooltip("Enable camera boundaries")]
        public bool UseBoundaries = false;
        
        [Tooltip("Minimum camera position")]
        public Vector3 MinBounds = new Vector3(-100f, -100f, -100f);
        
        [Tooltip("Maximum camera position")]
        public Vector3 MaxBounds = new Vector3(100f, 100f, 100f);
    }
}

