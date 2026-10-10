using UnityEngine;

namespace Bw.UseCases.Character
{
    public readonly struct Hit
    {
        public readonly float Damage;
        public readonly Vector2 Knockback;

        public Hit(float damage, Vector2 knockback)
        {
            Damage = damage;
            Knockback = knockback;
        }

        public static Hit Along(Vector2 direction, IHitConfig config) =>
            new(config.Damage, direction * config.Knockback);
    }
}
