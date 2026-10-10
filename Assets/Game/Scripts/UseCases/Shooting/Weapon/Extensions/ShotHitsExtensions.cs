using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class ShotHitsExtensions
    {
        public static bool TryFindForeign(this List<RaycastHit2D> hits, Transform shooter, out RaycastHit2D hit)
        {
            for (var index = 0; index < hits.Count; index++)
            {
                if (hits[index].transform.root == shooter)
                    continue;

                hit = hits[index];
                return true;
            }

            hit = default;
            return false;
        }
    }
}
