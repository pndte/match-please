using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Unity.Collections;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class ShotHitsExtensions
    {
        public static bool TryFindStruck(this NativeArray<PhysicsQuery.WorldCastResult> hits, IReadonlyMortal attacker, out PhysicsQuery.WorldCastResult hit)
        {
            for (var index = 0; index < hits.Length; index++)
            {
                var shape = hits[index].shape;
                if (shape.IsOneWay() || shape.TryGetMortal(out var struck) && struck == attacker)
                    continue;

                hit = hits[index];
                return true;
            }

            hit = default;
            return false;
        }
    }
}
