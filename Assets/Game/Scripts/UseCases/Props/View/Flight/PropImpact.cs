using UnityEngine;

namespace Bw.UseCases.Props.View.Flight
{
    public readonly struct PropImpact
    {
        public readonly Vector2 Point;
        public readonly Vector2 Normal;
        public readonly float Speed;

        public PropImpact(Vector2 point, Vector2 normal, float speed)
        {
            Point = point;
            Normal = normal;
            Speed = speed;
        }
    }
}
