using UnityEngine;

namespace Bw.UseCases.Mortality.Extensions
{
    public static class ImpactsExtensions
    {
        public static float Damage(this Impacts impacts)
        {
            var damage = 0f;
            foreach (var impact in impacts)
                damage += impact.Hit.Damage;

            return damage;
        }

        public static Vector2 Knockback(this Impacts impacts)
        {
            var knockback = Vector2.zero;
            foreach (var impact in impacts)
                knockback += impact.Hit.Knockback;

            return knockback;
        }
    }
}
