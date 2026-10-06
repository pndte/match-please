using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct ShotTrace
    {
        public readonly Vector2 From;
        public readonly Vector2 To;
        public readonly RaycastHit2D Hit;

        public ShotTrace(Vector2 from, Vector2 to, RaycastHit2D hit)
        {
            From = from;
            To = to;
            Hit = hit;
        }
    }
}
