using UnityEngine;

namespace Bw.UseCases.FogOfWar.View.Sight
{
    public readonly struct SightRay
    {
        public readonly float Angle;
        public readonly Vector2 End;
        public readonly Vector2 Normal;
        public readonly bool IsHit;

        private SightRay(
            float angle,
            Vector2 end,
            Vector2 normal,
            bool isHit)
        {
            Angle = angle;
            End = end;
            Normal = normal;
            IsHit = isHit;
        }

        public static SightRay Hit(float angle, Vector2 point, Vector2 normal) =>
            new(angle, point, normal, true);

        public static SightRay Miss(float angle, Vector2 end) =>
            new(angle, end, Vector2.zero, false);
    }
}
