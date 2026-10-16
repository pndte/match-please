using UnityEngine;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class HitExtensions
    {
        public static Hit Plus(this Hit hit, Hit other)
        {
            var weight = hit.Knockback.magnitude;
            var otherWeight = other.Knockback.magnitude;
            var total = weight + otherWeight;
            var point = total > 0f
                ? (hit.LocalPoint * weight + other.LocalPoint * otherWeight) / total
                : (hit.LocalPoint + other.LocalPoint) / 2f;
            return new Hit(hit.Damage + other.Damage, hit.Knockback + other.Knockback, point);
        }

        public static Hit At(this Hit hit, Vector2 localPoint) =>
            new(hit.Damage, hit.Knockback, localPoint);
    }
}
