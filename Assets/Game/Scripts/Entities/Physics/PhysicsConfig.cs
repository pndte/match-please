using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    [CreateAssetMenu(fileName = "PhysicsConfig", menuName = "Configs/PhysicsConfig")]
    public sealed class PhysicsConfig : ScriptableObject
    {
        [Min(1)] public int SubSteps = 4;
        [Min(0)] public int Workers;
        public PhysicsWorld.DrawOptions Drawing = PhysicsWorld.DrawOptions.Off;
    }
}
