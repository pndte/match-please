using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class LayerMaskExtensions
    {
        public static PhysicsMask AsPhysicsMask(this LayerMask mask) =>
            new() { bitMask = (uint)mask.value };
    }
}
