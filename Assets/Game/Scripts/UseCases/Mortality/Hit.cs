using UnityEngine;

namespace Bw.UseCases.Mortality
{
    public readonly struct Hit
    {
        public readonly float Damage;
        public readonly Vector2 Knockback;
        public readonly Vector2 LocalPoint; //TODO: место попадания — не параметр удара: в систему цели точку переводит стреляющий (InverseTransformPoint чужого коллайдера), конструктор без точки молча бьёт в начало координат цели, дробь усредняет точки. На этапе 4 (выстрелы на касты Box2D) — Contact (точка в системе ударенного тела, переводит само тело) и эффект событийной предикции Impact (Hit + Contact) вместо Hit, по записи на дробину; счёт попаданий тогда считает цели, а не записи

        public Hit(float damage, Vector2 knockback)
            : this(damage, knockback, Vector2.zero)
        {
        }

        public Hit(float damage, Vector2 knockback, Vector2 localPoint)
        {
            Damage = damage;
            Knockback = knockback;
            LocalPoint = localPoint;
        }

        public static Hit Along(Vector2 direction, IHitConfig config) =>
            new(config.Damage, direction * config.Knockback);
    }
}
