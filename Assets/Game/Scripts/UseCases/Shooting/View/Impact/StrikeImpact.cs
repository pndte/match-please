using UnityEngine;

namespace Bw.UseCases.Shooting.View.Impact
{
    public readonly struct StrikeImpact
    {
        public readonly Vector2 Point;
        public readonly Vector2 Direction;

        public StrikeImpact(Vector2 point, Vector2 direction)
        {
            Point = point;
            Direction = direction;
        }
    }
}
