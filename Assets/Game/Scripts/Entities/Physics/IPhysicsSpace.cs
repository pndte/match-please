using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics
{
    public interface IPhysicsSpace
    {
        public PhysicsWorld World { get; }
    }
}
