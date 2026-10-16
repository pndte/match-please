using JetBrains.Collections.Viewable;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IShovableCollection : IViewableMap<PhysicsBody, IShovable>
    {
    }
}
