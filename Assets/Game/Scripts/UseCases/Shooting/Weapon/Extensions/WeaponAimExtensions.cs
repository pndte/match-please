using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponAimExtensions
    {
        public static float TurnedTowards(this float aim, float target, float maxDegrees) =>
            Mathf.DeltaAngle(0f, Mathf.MoveTowardsAngle(aim, target, maxDegrees));
    }
}
