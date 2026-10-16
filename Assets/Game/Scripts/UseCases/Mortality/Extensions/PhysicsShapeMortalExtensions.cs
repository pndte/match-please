using Bw.Entities;
using Bw.Entities.Physics.Extensions;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class PhysicsShapeMortalExtensions
    {
        public static bool TryGetMortal(this PhysicsShape shape, out IReadonlyMortal mortal)
        {
            if (shape.TryGetComponent<IHolder<IReadonlyMortal>>(out var holder))
            {
                mortal = holder.Value;
                return true;
            }

            mortal = default;
            return false;
        }
    }
}
