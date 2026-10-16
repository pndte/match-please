using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class ShotHitsExtensions
    {
        public static bool TryFindForeign(this NativeArray<PhysicsQuery.WorldCastResult> hits, IReadonlyMortal shooter, out PhysicsQuery.WorldCastResult hit)
        {
            for (var index = 0; index < hits.Length; index++)
            {
                if (hits[index].shape.TryGetMortal(out var struck) && struck == shooter)
                    continue;

                hit = hits[index];
                return true;
            }

            hit = default;
            return false;
        }
    }
}
