using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "Configs/PhysicsConfig")]
    public sealed class PhysicsConfig : ScriptableObject
    {
        [Min(1)] public int SubSteps = 8;
        [Min(0)] public int Workers;

        [Header("Contacts")]
        [Min(1f)] public float ContactFrequency = 60f;
        [Min(0f)] public float ContactDamping = 3f;
        [Min(0f)] public float ContactSpeed = 10f;

        public PhysicsWorld.DrawOptions Drawing = PhysicsWorld.DrawOptions.Off;
    }
}
