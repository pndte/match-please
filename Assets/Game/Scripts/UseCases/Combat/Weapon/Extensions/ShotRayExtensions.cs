using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Extensions
{
    public static class ShotRayExtensions
    {
        public static ShotRay Turned(this ShotRay ray, float degrees) =>
            new(ray.Pivot, ray.Muzzle, Quaternion.Euler(0f, 0f, degrees) * ray.Direction);
    }
}
