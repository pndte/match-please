using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class ShovableCollection : ViewableMap<PhysicsBody, IShovable>, IShovableCollection
    {
    }
}
